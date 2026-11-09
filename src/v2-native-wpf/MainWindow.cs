using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using Forms = System.Windows.Forms;
using Drawing = System.Drawing;

namespace HPChargeLimiter
{
    public class MainWindow : Window
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        // Fixed Accent & Status Brushes (Frozen for GPU performance)
        private static readonly SolidColorBrush AccentPrimary = MakeBrush("#0284C7");
        private static readonly SolidColorBrush AccentLightDark = MakeBrush("#38BDF8");
        private static readonly SolidColorBrush AccentLightMode = MakeBrush("#0284C7");
        private static readonly SolidColorBrush ColorSuccess = MakeBrush("#059669");
        private static readonly SolidColorBrush ColorSuccessBright = MakeBrush("#10B981");
        private static readonly SolidColorBrush ColorSuccessLightMode = MakeBrush("#059669");
        private static readonly SolidColorBrush ColorWarn = MakeBrush("#D97706");
        private static readonly SolidColorBrush ColorWarnBright = MakeBrush("#F59E0B");
        private static readonly SolidColorBrush ColorWarnLightMode = MakeBrush("#D97706");
        private static readonly SolidColorBrush ColorDanger = MakeBrush("#DC2626");
        private static readonly SolidColorBrush ColorDangerDark = MakeBrush("#991B1B");
        private static readonly SolidColorBrush WhiteBrush = MakeBrush("#FFFFFF");

        // Theme-dependent Button Brushes (Updated on Dark/Light toggle)
        private static readonly SolidColorBrush DarkBtnInactiveBg = MakeBrush("#1E293B");
        private static readonly SolidColorBrush DarkBtnInactiveFg = MakeBrush("#E2E8F0");
        private static readonly SolidColorBrush LightBtnInactiveBg = MakeBrush("#CBD5E1");
        private static readonly SolidColorBrush LightBtnInactiveFg = MakeBrush("#0F172A");

        private SolidColorBrush _btnInactiveBg = DarkBtnInactiveBg;
        private SolidColorBrush _btnInactiveFg = DarkBtnInactiveFg;

        private static SolidColorBrush MakeBrush(string hex)
        {
            SolidColorBrush b = (SolidColorBrush)new BrushConverter().ConvertFromString(hex);
            b.Freeze();
            return b;
        }

        // State
        private GuiConfig _guiConfig;
        private ServiceConfig _svcConfig;
        private BatteryStatus _lastBattery = new BatteryStatus();
        private bool _isDark = true;
        private bool _isExiting = false;
        private bool _updatingSliders = false;
        private bool _updatingBypassToggle = false;
        private string _lastLogContent = "";
        private int _activeTabIndex = 0;
        private int _watchDurationSec = 30;
        private bool _watchRunning = false;

        private DispatcherTimer _refreshTimer;
        private DispatcherTimer _saveDebounceTimer;
        private Forms.NotifyIcon _trayIcon;
        private Forms.ToolStripMenuItem _trayMenuOpen;
        private Forms.ToolStripMenuItem _trayMenuBypass;
        private Forms.ToolStripMenuItem _trayMenuExit;
        private int _lastTrayKey = -1;

        // Tracked Dynamic Tooltips (key -> list of info badge buttons)
        private readonly Dictionary<string, List<FrameworkElement>> _tooltipTargets = new Dictionary<string, List<FrameworkElement>>();

        // Header Widgets
        private TextBlock _lblMainTitle;
        private TextBlock _lblSubTitle;
        private Button _btnThemeToggle;
        private Button _btnLangTr;
        private Button _btnLangEn;
        private Border _badgeAdminBorder;
        private TextBlock _lblBadgeAdmin;
        private Border _badgeSvcBorder;
        private TextBlock _lblBadgeSvc;
        private Button _btnRefresh;

        // Navigation Tab Buttons & Panels
        private readonly List<Button> _tabButtons = new List<Button>();
        private readonly List<UIElement> _tabPanels = new List<UIElement>();

        // Secondary buttons that adapt to Light/Dark theme
        private readonly List<Button> _secondaryButtons = new List<Button>();

        // Tab 1: Status & Limiter
        private TextBlock _lblCardBattery;
        private TextBlock _lblBatteryPct;
        private TextBlock _lblBatteryState;
        private Border _progressTrack;
        private Border _progressFill;
        private readonly Dictionary<string, TextBlock> _metricTitles = new Dictionary<string, TextBlock>();
        private readonly Dictionary<string, TextBlock> _metricValues = new Dictionary<string, TextBlock>();

        private TextBlock _lblCardLimiter;
        private TextBlock _lblPresets;
        private Button _btnPreset60;
        private Button _btnPreset80;
        private Button _btnPreset90;
        private TextBlock _lblStopTitle;
        private TextBlock _lblStopDesc;
        private Slider _sliderStop;
        private TextBlock _lblStartTitle;
        private TextBlock _lblStartDesc;
        private Slider _sliderStart;
        private TextBlock _lblOffModeTitle;
        private TextBlock _lblOffModeDesc;
        private readonly Dictionary<string, Button> _offModeButtons = new Dictionary<string, Button>();
        private CheckBox _chkBypass;
        private TextBlock _lblBypassSwitch;
        private Button _btnSave;

        // Tab 2: Advanced
        private TextBlock _lblAdvTitle;
        private TextBlock _lblRunModeTitle;
        private TextBlock _lblRunModeDesc;
        private readonly Dictionary<string, Button> _runModeButtons = new Dictionary<string, Button>();
        private TextBlock _lblOffMinTitle;
        private TextBlock _lblOffMinDesc;
        private Slider _sliderOffMin;
        private TextBlock _lblStopModeTitle;
        private TextBlock _lblStopModeDesc;
        private readonly Dictionary<string, Button> _stopModeButtons = new Dictionary<string, Button>();
        private TextBlock _lblIntervalTitle;
        private TextBlock _lblIntervalDesc;
        private Slider _sliderInterval;
        private Button _btnResetDefaults;

        // Tab 3: Tools & Diagnostics
        private TextBlock _lblWatchTitle;
        private TextBlock _lblWatchDesc;
        private readonly Dictionary<int, Button> _watchSecButtons = new Dictionary<int, Button>();
        private Button _btnStartWatch;
        private TextBox _txtWatchOut;
        private TextBlock _lblManualTitle;
        private TextBlock _lblManualDesc;
        private Button _btnSetInhibit;
        private Button _btnSetHold;
        private Button _btnSetAuto;
        private TextBlock _lblDiagTitle;
        private TextBlock _lblDiagDesc;
        private Button _btnRunDiag;
        private TextBox _txtDiagOut;

        // Tab 4: Logs
        private TextBlock _lblLogTitle;
        private CheckBox _chkLiveTail;
        private TextBlock _lblChkLiveTail;
        private Button _btnCopyLog;
        private Button _btnOpenFolder;
        private Button _btnClearLog;
        private TextBox _txtLogs;

        // Tab 5: System & Preferences
        private TextBlock _lblPrefTitle;
        private CheckBox _chkMinimizeTray;
        private TextBlock _lblSwTray;
        private CheckBox _chkAutostart;
        private TextBlock _lblSwAutostart;
        private TextBlock _lblAutostartInfo;
        private Button _btnElevate;
        private TextBlock _lblSvcMgrTitle;
        private Button _btnRestartSvc;
        private Button _btnStopSvc;
        private Button _btnStartSvc;
        private Button _btnUninstallSvc;
        private Button _btnFullCleanup;
        private TextBlock _lblAboutTitle;
        private TextBlock _lblAboutText;

        public MainWindow(bool startMinimized)
        {
            _guiConfig = Backend.LoadGuiConfig();
            _svcConfig = Backend.GetServiceConfig();
            I18n.CurrentLang = _guiConfig.Language;
            _isDark = !string.Equals(_guiConfig.Theme, "light", StringComparison.OrdinalIgnoreCase);

            Title = "HP Battery Charge Limiter";
            Width = 1040;
            Height = 750;
            MinWidth = 960;
            MinHeight = 680;
            FontFamily = new FontFamily("Segoe UI");
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            UseLayoutRounding = true;
            SnapsToDevicePixels = true;
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(this, TextRenderingMode.ClearType);

            try
            {
                System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
                using (Stream iconStream = asm.GetManifestResourceStream("HPChargeLimiter.app_icon.ico"))
                {
                    if (iconStream != null)
                    {
                        Icon = System.Windows.Media.Imaging.BitmapFrame.Create(
                            iconStream,
                            System.Windows.Media.Imaging.BitmapCreateOptions.PreservePixelFormat,
                            System.Windows.Media.Imaging.BitmapCacheOption.OnLoad);
                    }
                }
            }
            catch { }

            SourceInitialized += OnSourceInitialized;
            Closing += OnWindowClosing;

            ApplyThemeResources(_isDark);
            LoadCustomStyles();
            SetResourceReference(BackgroundProperty, "ResBgApp");
            SetResourceReference(ForegroundProperty, "ResTextPrimary");

            BuildUserInterface();
            ApplyTranslations();
            UpdateSegmentedHighlights();

            SetupSystemTray();

            _saveDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(350) };
            _saveDebounceTimer.Tick += (s, e) =>
            {
                _saveDebounceTimer.Stop();
                Task.Run(() => Backend.SaveGuiConfig(_guiConfig));
            };

            _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _refreshTimer.Tick += (s, e) => TriggerAsyncRefresh();
            _refreshTimer.Start();

            TriggerAsyncRefresh();

            if (startMinimized)
            {
                WindowState = WindowState.Minimized;
                Hide();
            }
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            UpdateDwmTitleBar();
        }

        private void UpdateDwmTitleBar()
        {
            try
            {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                if (hwnd == IntPtr.Zero) return;
                int dark = _isDark ? 1 : 0;
                DwmSetWindowAttribute(hwnd, 20, ref dark, sizeof(int));
                DwmSetWindowAttribute(hwnd, 19, ref dark, sizeof(int));
            }
            catch { }
        }

        private void ApplyThemeResources(bool dark)
        {
            if (dark)
            {
                _btnInactiveBg = DarkBtnInactiveBg;
                _btnInactiveFg = DarkBtnInactiveFg;

                Resources["ResBgApp"] = MakeBrush("#0B0F19");
                Resources["ResBgSurface"] = MakeBrush("#111827");
                Resources["ResBgCard"] = MakeBrush("#1F2937");
                Resources["ResBgCode"] = MakeBrush("#090D16");
                Resources["ResBorderSubtle"] = MakeBrush("#334155");
                Resources["ResSwitchOffBg"] = MakeBrush("#334155");
                Resources["ResTextPrimary"] = MakeBrush("#F8FAFC");
                Resources["ResTextSecondary"] = MakeBrush("#94A3B8");
                Resources["ResAccentText"] = AccentLightDark;
                Resources["ResInfoBadgeBg"] = MakeBrush("#1E3A5F");
                Resources["ResInfoBadgeFg"] = MakeBrush("#38BDF8");
                Resources["ResToolTipBg"] = MakeBrush("#0F172A");
                Resources["ResToolTipBorder"] = MakeBrush("#0284C7");
            }
            else
            {
                _btnInactiveBg = LightBtnInactiveBg;
                _btnInactiveFg = LightBtnInactiveFg;

                Resources["ResBgApp"] = MakeBrush("#F1F5F9");
                Resources["ResBgSurface"] = MakeBrush("#FFFFFF");
                Resources["ResBgCard"] = MakeBrush("#E2E8F0");
                Resources["ResBgCode"] = MakeBrush("#F8FAFC");
                Resources["ResBorderSubtle"] = MakeBrush("#CBD5E1");
                Resources["ResSwitchOffBg"] = MakeBrush("#94A3B8");
                Resources["ResTextPrimary"] = MakeBrush("#0F172A");
                Resources["ResTextSecondary"] = MakeBrush("#475569");
                Resources["ResAccentText"] = AccentLightMode;
                Resources["ResInfoBadgeBg"] = MakeBrush("#E0F2FE");
                Resources["ResInfoBadgeFg"] = MakeBrush("#0284C7");
                Resources["ResToolTipBg"] = MakeBrush("#FFFFFF");
                Resources["ResToolTipBorder"] = MakeBrush("#0284C7");
            }
        }

        private void ToggleTheme()
        {
            _isDark = !_isDark;
            _guiConfig.Theme = _isDark ? "dark" : "light";
            ApplyThemeResources(_isDark);
            UpdateDwmTitleBar();
            _btnThemeToggle.Content = _isDark ? "☀️" : "🌙";
            UpdateSegmentedHighlights();
            UpdateBatteryUi(_lastBattery);
            RefreshAllTooltips();
            ScheduleConfigSave();
        }

        private void LoadCustomStyles()
        {
            string xaml = @"
<ResourceDictionary xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <Style x:Key='ModernButton' TargetType='Button'>
        <Setter Property='Foreground' Value='#F8FAFC'/>
        <Setter Property='Background' Value='#1E293B'/>
        <Setter Property='BorderThickness' Value='0'/>
        <Setter Property='Padding' Value='10,4'/>
        <Setter Property='Cursor' Value='Hand'/>
        <Setter Property='FontWeight' Value='SemiBold'/>
        <Setter Property='FontSize' Value='12.5'/>
        <Setter Property='Template'>
            <Setter.Value>
                <ControlTemplate TargetType='Button'>
                    <Border x:Name='border' Background='{TemplateBinding Background}' CornerRadius='6' Padding='{TemplateBinding Padding}'>
                        <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property='IsMouseOver' Value='True'>
                            <Setter TargetName='border' Property='Opacity' Value='0.85'/>
                        </Trigger>
                        <Trigger Property='IsPressed' Value='True'>
                            <Setter TargetName='border' Property='Opacity' Value='0.72'/>
                        </Trigger>
                        <Trigger Property='IsEnabled' Value='False'>
                            <Setter TargetName='border' Property='Opacity' Value='0.45'/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <Style x:Key='InfoBadgeButton' TargetType='Button'>
        <Setter Property='Width' Value='18'/>
        <Setter Property='Height' Value='18'/>
        <Setter Property='Cursor' Value='Hand'/>
        <Setter Property='Margin' Value='6,0,0,0'/>
        <Setter Property='VerticalAlignment' Value='Center'/>
        <Setter Property='Template'>
            <Setter.Value>
                <ControlTemplate TargetType='Button'>
                    <Border x:Name='bdr' Background='{DynamicResource ResInfoBadgeBg}' CornerRadius='9' BorderBrush='#0284C7' BorderThickness='1'>
                        <TextBlock x:Name='txt' Text='i' FontSize='11' FontWeight='Bold' FontFamily='Consolas' Foreground='{DynamicResource ResInfoBadgeFg}' HorizontalAlignment='Center' VerticalAlignment='Center'/>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property='IsMouseOver' Value='True'>
                            <Setter TargetName='bdr' Property='Background' Value='#0284C7'/>
                            <Setter TargetName='txt' Property='Foreground' Value='#FFFFFF'/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <Style x:Key='ModernSlider' TargetType='Slider'>
        <Setter Property='Focusable' Value='False'/>
        <Setter Property='SnapsToDevicePixels' Value='True'/>
        <Setter Property='Template'>
            <Setter.Value>
                <ControlTemplate TargetType='Slider'>
                    <Grid VerticalAlignment='Center' Height='24'>
                        <Border Height='6' Background='{DynamicResource ResBorderSubtle}' CornerRadius='3' VerticalAlignment='Center'/>
                        <Track x:Name='PART_Track'>
                            <Track.DecreaseRepeatButton>
                                <RepeatButton Command='Slider.DecreaseLarge' Opacity='0' Focusable='False'/>
                            </Track.DecreaseRepeatButton>
                            <Track.IncreaseRepeatButton>
                                <RepeatButton Command='Slider.IncreaseLarge' Opacity='0' Focusable='False'/>
                            </Track.IncreaseRepeatButton>
                            <Track.Thumb>
                                <Thumb Width='18' Height='18' Cursor='Hand'>
                                    <Thumb.Template>
                                        <ControlTemplate TargetType='Thumb'>
                                            <Border Background='#38BDF8' BorderBrush='#0284C7' BorderThickness='2' CornerRadius='9'/>
                                        </ControlTemplate>
                                    </Thumb.Template>
                                </Thumb>
                            </Track.Thumb>
                        </Track>
                    </Grid>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <Style x:Key='SwitchCheckBox' TargetType='CheckBox'>
        <Setter Property='Cursor' Value='Hand'/>
        <Setter Property='Foreground' Value='{DynamicResource ResTextPrimary}'/>
        <Setter Property='Template'>
            <Setter.Value>
                <ControlTemplate TargetType='CheckBox'>
                    <StackPanel Orientation='Horizontal' VerticalAlignment='Center'>
                        <Border x:Name='switchBg' Width='42' Height='22' CornerRadius='11' Background='{DynamicResource ResSwitchOffBg}' Padding='2'>
                            <Border x:Name='switchKnob' Width='18' Height='18' CornerRadius='9' Background='#FFFFFF' HorizontalAlignment='Left'/>
                        </Border>
                        <ContentPresenter Margin='10,0,0,0' VerticalAlignment='Center'/>
                    </StackPanel>
                    <ControlTemplate.Triggers>
                        <Trigger Property='IsChecked' Value='True'>
                            <Setter TargetName='switchBg' Property='Background' Value='#0284C7'/>
                            <Setter TargetName='switchKnob' Property='HorizontalAlignment' Value='Right'/>
                        </Trigger>
                        <Trigger Property='IsEnabled' Value='False'>
                            <Setter TargetName='switchBg' Property='Opacity' Value='0.5'/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
</ResourceDictionary>";
            Resources.MergedDictionaries.Add((ResourceDictionary)XamlReader.Parse(xaml));
        }

        private Button CreateStyledButton(string text, SolidColorBrush bg, RoutedEventHandler onClick, double height = 32, bool isSecondary = false)
        {
            Button b = new Button
            {
                Style = (Style)Resources["ModernButton"],
                Content = text,
                Background = isSecondary ? _btnInactiveBg : bg,
                Foreground = isSecondary ? _btnInactiveFg : WhiteBrush,
                Height = height
            };
            if (onClick != null) b.Click += onClick;
            if (isSecondary) _secondaryButtons.Add(b);
            return b;
        }

        private ToolTip BuildStyledToolTip(string tipKey)
        {
            Border bdr = new Border
            {
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(12, 9, 12, 9),
                MaxWidth = 360
            };
            bdr.SetResourceReference(Border.BackgroundProperty, "ResToolTipBg");
            bdr.SetResourceReference(Border.BorderBrushProperty, "ResToolTipBorder");

            TextBlock tb = new TextBlock
            {
                Text = I18n.Get(tipKey),
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 17.5
            };
            tb.SetResourceReference(TextBlock.ForegroundProperty, "ResTextPrimary");
            bdr.Child = tb;

            return new ToolTip
            {
                Content = bdr,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0),
                Placement = PlacementMode.Bottom,
                VerticalOffset = 6
            };
        }

        private void AttachTechnicalInfo(FrameworkElement element, string tipKey)
        {
            ToolTipService.SetInitialShowDelay(element, 60);
            ToolTipService.SetShowDuration(element, 45000);
            ToolTipService.SetBetweenShowDelay(element, 50);
            element.ToolTip = BuildStyledToolTip(tipKey);

            List<FrameworkElement> list;
            if (!_tooltipTargets.TryGetValue(tipKey, out list))
            {
                list = new List<FrameworkElement>();
                _tooltipTargets[tipKey] = list;
            }
            if (!list.Contains(element))
                list.Add(element);
        }

        private void RefreshAllTooltips()
        {
            foreach (var kv in _tooltipTargets)
            {
                string tipKey = kv.Key;
                foreach (FrameworkElement el in kv.Value)
                {
                    el.ToolTip = BuildStyledToolTip(tipKey);
                }
            }
        }

        private Button CreateInfoBadgeButton(string tipKey)
        {
            Button infoBtn = new Button
            {
                Style = (Style)Resources["InfoBadgeButton"]
            };
            AttachTechnicalInfo(infoBtn, tipKey);
            infoBtn.Click += (s, e) => ShowTechnicalPopup(tipKey);
            return infoBtn;
        }

        /// <summary>
        /// Places the setting title next to an 'i' info badge button.
        /// Tooltips and click popups are attached ONLY to the 'i' button, NOT the title text.
        /// </summary>
        private StackPanel CreateHeaderWithInfoBadge(TextBlock titleLabel, string tipKey, Thickness margin)
        {
            StackPanel sp = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = margin
            };
            titleLabel.VerticalAlignment = VerticalAlignment.Center;
            sp.Children.Add(titleLabel);
            sp.Children.Add(CreateInfoBadgeButton(tipKey));
            return sp;
        }

        private void ShowTechnicalPopup(string tipKey)
        {
            MessageBox.Show(this, I18n.Get(tipKey), I18n.Get("msg_info"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private Border CreateThemedBorder(string bgResKey, double radius, Thickness padding, Thickness margin = default(Thickness))
        {
            Border b = new Border
            {
                CornerRadius = new CornerRadius(radius),
                Padding = padding,
                Margin = margin
            };
            b.SetResourceReference(Border.BackgroundProperty, bgResKey);
            return b;
        }

        private TextBlock CreateThemedText(string fgResKey, double fontSize, FontWeight weight, Thickness margin = default(Thickness))
        {
            TextBlock tb = new TextBlock
            {
                FontSize = fontSize,
                FontWeight = weight,
                Margin = margin
            };
            tb.SetResourceReference(TextBlock.ForegroundProperty, fgResKey);
            return tb;
        }

        private void BuildUserInterface()
        {
            Grid root = new Grid { Margin = new Thickness(16, 12, 16, 14) };
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(70) });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(46) });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            Content = root;

            // 1. HEADER (2-Column Grid so Right Controls are NEVER clipped or shifted!)
            Border headerCard = CreateThemedBorder("ResBgSurface", 10, new Thickness(18, 10, 18, 10));
            Grid headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerCard.Child = headerGrid;

            StackPanel titleStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 12, 0) };
            _lblMainTitle = CreateThemedText("ResTextPrimary", 20, FontWeights.Bold);
            _lblSubTitle = CreateThemedText("ResTextSecondary", 11.5, FontWeights.Normal, new Thickness(0, 2, 0, 0));
            _lblSubTitle.TextTrimming = TextTrimming.CharacterEllipsis;
            titleStack.Children.Add(_lblMainTitle);
            titleStack.Children.Add(_lblSubTitle);
            Grid.SetColumn(titleStack, 0);
            headerGrid.Children.Add(titleStack);

            // Fixed-Width Right Controls
            StackPanel rightControls = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(rightControls, 1);

            // Theme Toggle Button (Fixed Width = 36)
            _btnThemeToggle = CreateStyledButton(_isDark ? "☀️" : "🌙", _btnInactiveBg, (s, e) => ToggleTheme(), 30, true);
            _btnThemeToggle.Width = 36;
            _btnThemeToggle.Padding = new Thickness(0);
            _btnThemeToggle.FontSize = 14;
            _btnThemeToggle.Margin = new Thickness(0, 0, 8, 0);
            rightControls.Children.Add(_btnThemeToggle);

            // Language Pill (Fixed Width = 96)
            Border langBox = CreateThemedBorder("ResBgCard", 7, new Thickness(3), new Thickness(0, 0, 8, 0));
            StackPanel langRow = new StackPanel { Orientation = Orientation.Horizontal };
            _btnLangTr = CreateStyledButton("TR", AccentPrimary, (s, e) => SwitchLanguage("tr"), 26);
            _btnLangTr.Width = 44;
            _btnLangTr.Padding = new Thickness(0);
            _btnLangTr.FontSize = 12;
            _btnLangEn = CreateStyledButton("EN", _btnInactiveBg, (s, e) => SwitchLanguage("en"), 26);
            _btnLangEn.Width = 44;
            _btnLangEn.Padding = new Thickness(0);
            _btnLangEn.FontSize = 12;
            _btnLangEn.Margin = new Thickness(2, 0, 0, 0);
            langRow.Children.Add(_btnLangTr);
            langRow.Children.Add(_btnLangEn);
            langBox.Child = langRow;
            rightControls.Children.Add(langBox);

            // Admin Badge (Fixed Width = 138)
            bool isAdmin = Backend.IsAdmin();
            _badgeAdminBorder = new Border
            {
                Background = isAdmin ? ColorSuccess : _btnInactiveBg,
                CornerRadius = new CornerRadius(6),
                Width = 138,
                Height = 30,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            _lblBadgeAdmin = new TextBlock
            {
                FontSize = 11.5,
                FontWeight = FontWeights.Bold,
                Foreground = isAdmin ? WhiteBrush : _btnInactiveFg,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            _badgeAdminBorder.Child = _lblBadgeAdmin;
            rightControls.Children.Add(_badgeAdminBorder);

            // Service Status Badge (Fixed Width = 148)
            _badgeSvcBorder = new Border
            {
                Background = ColorSuccess,
                CornerRadius = new CornerRadius(6),
                Width = 148,
                Height = 30,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            _lblBadgeSvc = new TextBlock
            {
                FontSize = 11.5,
                FontWeight = FontWeights.Bold,
                Foreground = WhiteBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            _badgeSvcBorder.Child = _lblBadgeSvc;
            rightControls.Children.Add(_badgeSvcBorder);

            // Refresh Button (Fixed Width = 88)
            _btnRefresh = CreateStyledButton("⟳ Yenile", _btnInactiveBg, (s, e) => TriggerAsyncRefresh(), 30, true);
            _btnRefresh.Width = 88;
            _btnRefresh.Padding = new Thickness(0);
            rightControls.Children.Add(_btnRefresh);

            headerGrid.Children.Add(rightControls);
            Grid.SetRow(headerCard, 0);
            root.Children.Add(headerCard);

            // 2. NAVIGATION BAR (5 TABS in Fixed UniformGrid = 5 equal columns)
            Border navBar = CreateThemedBorder("ResBgSurface", 8, new Thickness(4), new Thickness(0, 8, 0, 4));
            UniformGrid navGrid = new UniformGrid { Columns = 5 };
            string[] tabKeys = { "tab_status", "tab_advanced", "tab_tools", "tab_logs", "tab_settings" };
            for (int i = 0; i < tabKeys.Length; i++)
            {
                int idx = i;
                Button tb = CreateStyledButton(I18n.Get(tabKeys[i]), _btnInactiveBg, (s, e) => SelectTab(idx), 30);
                tb.Margin = new Thickness(2, 0, 2, 0);
                tb.Padding = new Thickness(4, 0, 4, 0);
                _tabButtons.Add(tb);
                navGrid.Children.Add(tb);
            }
            navBar.Child = navGrid;
            Grid.SetRow(navBar, 1);
            root.Children.Add(navBar);

            // 3. CONTENT AREA
            Border contentContainer = CreateThemedBorder("ResBgSurface", 10, new Thickness(12), new Thickness(0, 4, 0, 0));
            Grid tabHost = new Grid();
            contentContainer.Child = tabHost;

            UIElement p0 = BuildStatusTab();
            UIElement p1 = BuildAdvancedTab();
            UIElement p2 = BuildToolsTab();
            UIElement p3 = BuildLogsTab();
            UIElement p4 = BuildSettingsTab();

            _tabPanels.AddRange(new[] { p0, p1, p2, p3, p4 });
            foreach (UIElement p in _tabPanels)
                tabHost.Children.Add(p);

            Grid.SetRow(contentContainer, 2);
            root.Children.Add(contentContainer);

            SelectTab(0);
        }

        private void SelectTab(int index)
        {
            _activeTabIndex = index;
            for (int i = 0; i < _tabPanels.Count; i++)
            {
                _tabPanels[i].Visibility = (i == index) ? Visibility.Visible : Visibility.Collapsed;
            }
            UpdateSegmentedHighlights();
            if (index == 3)
                RefreshLogsUi(true);
        }

        private void SetButtonActiveState(Button btn, bool active)
        {
            btn.Background = active ? AccentPrimary : _btnInactiveBg;
            btn.Foreground = active ? WhiteBrush : _btnInactiveFg;
        }

        // ================= TAB 1: STATUS & LIMITER =================
        private UIElement BuildStatusTab()
        {
            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // LEFT CARD: LIVE BATTERY
            Border leftCard = CreateThemedBorder("ResBgCard", 10, new Thickness(16, 14, 16, 14));
            DockPanel leftDock = new DockPanel();
            leftCard.Child = leftDock;

            _lblCardBattery = CreateThemedText("ResAccentText", 16, FontWeights.Bold, new Thickness(0, 0, 0, 10));
            DockPanel.SetDock(_lblCardBattery, Dock.Top);
            leftDock.Children.Add(_lblCardBattery);

            // Big % Box
            Border pctBox = CreateThemedBorder("ResBgSurface", 8, new Thickness(16, 8, 16, 8), new Thickness(0, 0, 0, 8));
            DockPanel pctDock = new DockPanel();
            _lblBatteryPct = CreateThemedText("ResTextPrimary", 42, FontWeights.Bold);
            _lblBatteryPct.Text = "%--";
            _lblBatteryPct.VerticalAlignment = VerticalAlignment.Center;
            _lblBatteryState = CreateThemedText("ResAccentText", 13, FontWeights.Bold);
            _lblBatteryState.VerticalAlignment = VerticalAlignment.Center;
            _lblBatteryState.HorizontalAlignment = HorizontalAlignment.Right;
            _lblBatteryState.TextWrapping = TextWrapping.Wrap;
            DockPanel.SetDock(_lblBatteryPct, Dock.Left);
            DockPanel.SetDock(_lblBatteryState, Dock.Right);
            pctDock.Children.Add(_lblBatteryPct);
            pctDock.Children.Add(_lblBatteryState);
            pctBox.Child = pctDock;
            DockPanel.SetDock(pctBox, Dock.Top);
            leftDock.Children.Add(pctBox);

            // Progress Bar
            _progressTrack = CreateThemedBorder("ResBgSurface", 7, new Thickness(0), new Thickness(0, 2, 0, 10));
            _progressTrack.Height = 14;
            _progressTrack.ClipToBounds = true;
            _progressFill = new Border { Background = AccentPrimary, Height = 14, CornerRadius = new CornerRadius(7), HorizontalAlignment = HorizontalAlignment.Left, Width = 0 };
            _progressTrack.Child = _progressFill;
            _progressTrack.SizeChanged += (s, e) => UpdateProgressBarWidth(_lastBattery.Percent);
            DockPanel.SetDock(_progressTrack, Dock.Top);
            leftDock.Children.Add(_progressTrack);

            // Metrics Grid: 2 Columns x 5 Rows where each cell stacks Title on top & Value wrapping downward!
            Border metricsBox = CreateThemedBorder("ResBgSurface", 8, new Thickness(14, 10, 14, 10));
            Grid mGrid = new Grid();
            mGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            mGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            for (int r = 0; r < 5; r++)
                mGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            string[,] metricDefs = {
                { "metric_ac", "ac", "" },
                { "metric_power", "power", "" },
                { "metric_energy", "energy", "" },
                { "metric_voltage", "voltage", "" },
                { "metric_health", "health", "" },
                { "metric_cycles", "cycles", "" },
                { "metric_model", "model", "" },
                { "metric_bypass", "bypass", "tip_bypass" },
                { "metric_firmware", "firmware", "tip_firmware" },
                { "metric_last_off", "last_off", "tip_last_off" }
            };

            for (int i = 0; i < metricDefs.GetLength(0); i++)
            {
                string titleKey = metricDefs[i, 0];
                string valKey = metricDefs[i, 1];
                string tipKey = metricDefs[i, 2];

                int row = i / 2;
                int col = (i % 2) * 2;

                StackPanel cellStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(2, 2, 2, 2) };

                StackPanel titleRow = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
                TextBlock tLbl = CreateThemedText("ResTextSecondary", 11, FontWeights.Normal);
                tLbl.VerticalAlignment = VerticalAlignment.Center;
                titleRow.Children.Add(tLbl);

                if (!string.IsNullOrEmpty(tipKey))
                {
                    Button infoBtn = CreateInfoBadgeButton(tipKey);
                    infoBtn.Width = 15;
                    infoBtn.Height = 15;
                    infoBtn.Margin = new Thickness(5, 0, 0, 0);
                    titleRow.Children.Add(infoBtn);
                }

                TextBlock vLbl = CreateThemedText("ResTextPrimary", 12.5, FontWeights.Bold, new Thickness(0, 2, 0, 0));
                vLbl.Text = "--";
                vLbl.TextWrapping = TextWrapping.Wrap;

                cellStack.Children.Add(titleRow);
                cellStack.Children.Add(vLbl);

                _metricTitles[titleKey] = tLbl;
                _metricValues[valKey] = vLbl;

                Grid.SetRow(cellStack, row);
                Grid.SetColumn(cellStack, col);
                mGrid.Children.Add(cellStack);
            }

            metricsBox.Child = mGrid;
            leftDock.Children.Add(metricsBox);
            Grid.SetColumn(leftCard, 0);
            g.Children.Add(leftCard);

            // RIGHT CARD: LIMITER SETTINGS
            Border rightCard = CreateThemedBorder("ResBgCard", 10, new Thickness(16, 14, 16, 14));
            DockPanel rightDock = new DockPanel();
            rightCard.Child = rightDock;

            _lblCardLimiter = CreateThemedText("ResAccentText", 16, FontWeights.Bold, new Thickness(0, 0, 0, 8));
            DockPanel.SetDock(_lblCardLimiter, Dock.Top);
            rightDock.Children.Add(_lblCardLimiter);

            // Bottom Save & Bypass
            StackPanel bottomStack = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };
            DockPanel.SetDock(bottomStack, Dock.Bottom);

            StackPanel bypassRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10), VerticalAlignment = VerticalAlignment.Center };
            _lblBypassSwitch = CreateThemedText("ResTextPrimary", 12.5, FontWeights.Bold);
            _chkBypass = new CheckBox
            {
                Style = (Style)Resources["SwitchCheckBox"],
                Content = _lblBypassSwitch
            };
            _chkBypass.Click += OnToggleBypassClick;

            bypassRow.Children.Add(_chkBypass);
            bypassRow.Children.Add(CreateInfoBadgeButton("tip_bypass"));
            bottomStack.Children.Add(bypassRow);

            _btnSave = CreateStyledButton("💾 Save", AccentPrimary, OnSaveSettingsClick, 40);
            _btnSave.FontSize = 14;
            _btnSave.FontWeight = FontWeights.Bold;
            bottomStack.Children.Add(_btnSave);
            rightDock.Children.Add(bottomStack);

            // Middle Stack (Presets + Sliders + OffMode)
            StackPanel midStack = new StackPanel();

            // Quick Presets Box (Title + 'i' on top, 3 full-width buttons below so nothing clips!)
            Border presetBox = CreateThemedBorder("ResBgSurface", 8, new Thickness(14, 8, 14, 8), new Thickness(0, 0, 0, 6));
            StackPanel presetStack = new StackPanel();
            _lblPresets = CreateThemedText("ResTextPrimary", 12, FontWeights.Bold);
            presetStack.Children.Add(CreateHeaderWithInfoBadge(_lblPresets, "tip_presets", new Thickness(0, 0, 0, 6)));

            UniformGrid presetGrid = new UniformGrid { Columns = 3 };
            _btnPreset60 = CreateStyledButton("%60", _btnInactiveBg, (s, e) => ApplyPreset(60, 55), 28);
            _btnPreset60.FontSize = 11.5;
            _btnPreset60.Padding = new Thickness(4, 0, 4, 0);
            _btnPreset60.Margin = new Thickness(0, 0, 3, 0);
            presetGrid.Children.Add(_btnPreset60);

            _btnPreset80 = CreateStyledButton("%80", _btnInactiveBg, (s, e) => ApplyPreset(80, 75), 28);
            _btnPreset80.FontSize = 11.5;
            _btnPreset80.Padding = new Thickness(4, 0, 4, 0);
            _btnPreset80.Margin = new Thickness(2, 0, 2, 0);
            presetGrid.Children.Add(_btnPreset80);

            _btnPreset90 = CreateStyledButton("%90", _btnInactiveBg, (s, e) => ApplyPreset(90, 85), 28);
            _btnPreset90.FontSize = 11.5;
            _btnPreset90.Padding = new Thickness(4, 0, 4, 0);
            _btnPreset90.Margin = new Thickness(3, 0, 0, 0);
            presetGrid.Children.Add(_btnPreset90);

            presetStack.Children.Add(presetGrid);
            presetBox.Child = presetStack;
            midStack.Children.Add(presetBox);

            // Stop % Box
            Border stopBox = CreateThemedBorder("ResBgSurface", 8, new Thickness(14, 8, 14, 8), new Thickness(0, 0, 0, 6));
            StackPanel stopStack = new StackPanel();
            _lblStopTitle = CreateThemedText("ResTextPrimary", 13, FontWeights.Bold);
            _lblStopDesc = CreateThemedText("ResTextSecondary", 11, FontWeights.Normal, new Thickness(0, 2, 0, 4));
            _lblStopDesc.TextWrapping = TextWrapping.Wrap;
            _sliderStop = new Slider
            {
                Style = (Style)Resources["ModernSlider"],
                Minimum = 50, Maximum = 95,
                Value = _svcConfig.Stop,
                IsSnapToTickEnabled = true, TickFrequency = 1
            };
            _sliderStop.ValueChanged += OnStopSliderChanged;
            stopStack.Children.Add(CreateHeaderWithInfoBadge(_lblStopTitle, "tip_stop", new Thickness(0)));
            stopStack.Children.Add(_lblStopDesc);
            stopStack.Children.Add(_sliderStop);
            stopBox.Child = stopStack;
            midStack.Children.Add(stopBox);

            // Start % Box
            Border startBox = CreateThemedBorder("ResBgSurface", 8, new Thickness(14, 8, 14, 8), new Thickness(0, 0, 0, 6));
            StackPanel startStack = new StackPanel();
            _lblStartTitle = CreateThemedText("ResTextPrimary", 13, FontWeights.Bold);
            _lblStartDesc = CreateThemedText("ResTextSecondary", 11, FontWeights.Normal, new Thickness(0, 2, 0, 4));
            _lblStartDesc.TextWrapping = TextWrapping.Wrap;
            _sliderStart = new Slider
            {
                Style = (Style)Resources["ModernSlider"],
                Minimum = 40, Maximum = 90,
                Value = _svcConfig.Start,
                IsSnapToTickEnabled = true, TickFrequency = 1
            };
            _sliderStart.ValueChanged += OnStartSliderChanged;
            startStack.Children.Add(CreateHeaderWithInfoBadge(_lblStartTitle, "tip_start", new Thickness(0)));
            startStack.Children.Add(_lblStartDesc);
            startStack.Children.Add(_sliderStart);
            startBox.Child = startStack;
            midStack.Children.Add(startBox);

            // Off-Mode Box
            Border offBox = CreateThemedBorder("ResBgSurface", 8, new Thickness(14, 8, 14, 8));
            StackPanel offStack = new StackPanel();
            _lblOffModeTitle = CreateThemedText("ResTextPrimary", 12.5, FontWeights.Bold);
            UniformGrid offGrid = new UniformGrid { Columns = 4, Margin = new Thickness(0, 0, 0, 6) };
            foreach (string m in new[] { "hold", "inhibit", "auto", "keep" })
            {
                string mode = m;
                Button b = CreateStyledButton(mode, _btnInactiveBg, (s, e) => SetOffMode(mode), 28);
                b.Margin = new Thickness(2, 0, 2, 0);
                _offModeButtons[mode] = b;
                offGrid.Children.Add(b);
            }
            _lblOffModeDesc = CreateThemedText("ResAccentText", 11, FontWeights.Normal);
            _lblOffModeDesc.TextWrapping = TextWrapping.Wrap;
            offStack.Children.Add(CreateHeaderWithInfoBadge(_lblOffModeTitle, "tip_off_mode", new Thickness(0, 0, 0, 6)));
            offStack.Children.Add(offGrid);
            offStack.Children.Add(_lblOffModeDesc);
            offBox.Child = offStack;
            midStack.Children.Add(offBox);

            rightDock.Children.Add(midStack);
            Grid.SetColumn(rightCard, 2);
            g.Children.Add(rightCard);

            return g;
        }

        // ================= TAB 2: ADVANCED SETTINGS =================
        private UIElement BuildAdvancedTab()
        {
            Border card = CreateThemedBorder("ResBgCard", 10, new Thickness(18, 14, 18, 14));
            StackPanel stack = new StackPanel();
            card.Child = stack;

            _lblAdvTitle = CreateThemedText("ResAccentText", 16, FontWeights.Bold, new Thickness(0, 0, 0, 10));
            stack.Children.Add(_lblAdvTitle);

            // 1. Run-Mode
            Border rmBox = CreateThemedBorder("ResBgSurface", 8, new Thickness(14, 8, 14, 8), new Thickness(0, 0, 0, 8));
            StackPanel rmStack = new StackPanel();
            _lblRunModeTitle = CreateThemedText("ResTextPrimary", 12.5, FontWeights.Bold);
            UniformGrid rmGrid = new UniformGrid { Columns = 3, Margin = new Thickness(0, 0, 0, 6) };
            foreach (string m in new[] { "inhibit", "hold", "auto" })
            {
                string mode = m;
                Button b = CreateStyledButton(mode, _btnInactiveBg, (s, e) => SetRunMode(mode), 28);
                b.Margin = new Thickness(2, 0, 2, 0);
                _runModeButtons[mode] = b;
                rmGrid.Children.Add(b);
            }
            _lblRunModeDesc = CreateThemedText("ResAccentText", 11, FontWeights.Normal);
            _lblRunModeDesc.TextWrapping = TextWrapping.Wrap;
            rmStack.Children.Add(CreateHeaderWithInfoBadge(_lblRunModeTitle, "tip_run_mode", new Thickness(0, 0, 0, 6)));
            rmStack.Children.Add(rmGrid);
            rmStack.Children.Add(_lblRunModeDesc);
            rmBox.Child = rmStack;
            stack.Children.Add(rmBox);

            // 2. Off-Min %
            Border omBox = CreateThemedBorder("ResBgSurface", 8, new Thickness(14, 8, 14, 8), new Thickness(0, 0, 0, 8));
            StackPanel omStack = new StackPanel();
            _lblOffMinTitle = CreateThemedText("ResTextPrimary", 12.5, FontWeights.Bold);
            _lblOffMinDesc = CreateThemedText("ResTextSecondary", 11, FontWeights.Normal, new Thickness(0, 2, 0, 4));
            _lblOffMinDesc.TextWrapping = TextWrapping.Wrap;
            _sliderOffMin = new Slider
            {
                Style = (Style)Resources["ModernSlider"],
                Minimum = 0, Maximum = 80,
                Value = _svcConfig.OffMin,
                IsSnapToTickEnabled = true, TickFrequency = 1
            };
            _sliderOffMin.ValueChanged += OnOffMinSliderChanged;
            omStack.Children.Add(CreateHeaderWithInfoBadge(_lblOffMinTitle, "tip_off_min", new Thickness(0)));
            omStack.Children.Add(_lblOffMinDesc);
            omStack.Children.Add(_sliderOffMin);
            omBox.Child = omStack;
            stack.Children.Add(omBox);

            // 3. Stop-Mode
            Border smBox = CreateThemedBorder("ResBgSurface", 8, new Thickness(14, 8, 14, 8), new Thickness(0, 0, 0, 8));
            StackPanel smStack = new StackPanel();
            _lblStopModeTitle = CreateThemedText("ResTextPrimary", 12.5, FontWeights.Bold);
            UniformGrid smGrid = new UniformGrid { Columns = 4, Margin = new Thickness(0, 0, 0, 6) };
            foreach (string m in new[] { "auto", "inhibit", "hold", "keep" })
            {
                string mode = m;
                Button b = CreateStyledButton(mode, _btnInactiveBg, (s, e) => SetStopMode(mode), 28);
                b.Margin = new Thickness(2, 0, 2, 0);
                _stopModeButtons[mode] = b;
                smGrid.Children.Add(b);
            }
            _lblStopModeDesc = CreateThemedText("ResAccentText", 11, FontWeights.Normal);
            _lblStopModeDesc.TextWrapping = TextWrapping.Wrap;
            smStack.Children.Add(CreateHeaderWithInfoBadge(_lblStopModeTitle, "tip_stop_mode", new Thickness(0, 0, 0, 6)));
            smStack.Children.Add(smGrid);
            smStack.Children.Add(_lblStopModeDesc);
            smBox.Child = smStack;
            stack.Children.Add(smBox);

            // 4. Polling Interval
            Border intBox = CreateThemedBorder("ResBgSurface", 8, new Thickness(14, 8, 14, 8), new Thickness(0, 0, 0, 10));
            StackPanel intStack = new StackPanel();
            _lblIntervalTitle = CreateThemedText("ResTextPrimary", 12.5, FontWeights.Bold);
            _lblIntervalDesc = CreateThemedText("ResTextSecondary", 11, FontWeights.Normal, new Thickness(0, 2, 0, 4));
            _lblIntervalDesc.TextWrapping = TextWrapping.Wrap;
            _sliderInterval = new Slider
            {
                Style = (Style)Resources["ModernSlider"],
                Minimum = 5, Maximum = 120,
                Value = _svcConfig.Interval,
                IsSnapToTickEnabled = true, TickFrequency = 1
            };
            _sliderInterval.ValueChanged += OnIntervalSliderChanged;
            intStack.Children.Add(CreateHeaderWithInfoBadge(_lblIntervalTitle, "tip_interval", new Thickness(0)));
            intStack.Children.Add(_lblIntervalDesc);
            intStack.Children.Add(_sliderInterval);
            intBox.Child = intStack;
            stack.Children.Add(intBox);

            // Reset Defaults Button
            _btnResetDefaults = CreateStyledButton("Reset", _btnInactiveBg, OnResetDefaultsClick, 32, true);
            _btnResetDefaults.HorizontalAlignment = HorizontalAlignment.Right;
            _btnResetDefaults.MinWidth = 240;
            stack.Children.Add(_btnResetDefaults);

            return card;
        }

        // ================= TAB 3: TOOLS & DIAGNOSTICS =================
        private UIElement BuildToolsTab()
        {
            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // LEFT: Watch Test & Manual Override
            Border leftCard = CreateThemedBorder("ResBgCard", 10, new Thickness(16, 14, 16, 14));
            DockPanel leftDock = new DockPanel();
            leftCard.Child = leftDock;

            _lblWatchTitle = CreateThemedText("ResAccentText", 15, FontWeights.Bold);
            StackPanel watchHdr = CreateHeaderWithInfoBadge(_lblWatchTitle, "tip_watch", new Thickness(0, 0, 0, 4));
            DockPanel.SetDock(watchHdr, Dock.Top);
            leftDock.Children.Add(watchHdr);

            _lblWatchDesc = CreateThemedText("ResTextSecondary", 11, FontWeights.Normal, new Thickness(0, 0, 0, 8));
            _lblWatchDesc.TextWrapping = TextWrapping.Wrap;
            DockPanel.SetDock(_lblWatchDesc, Dock.Top);
            leftDock.Children.Add(_lblWatchDesc);

            // Watch control bar
            Border watchBar = CreateThemedBorder("ResBgSurface", 8, new Thickness(10, 6, 10, 6), new Thickness(0, 0, 0, 8));
            DockPanel watchBarDock = new DockPanel();
            StackPanel secRow = new StackPanel { Orientation = Orientation.Horizontal };
            foreach (int sec in new[] { 10, 30, 60 })
            {
                int sVal = sec;
                Button b = CreateStyledButton(sVal + "s", _btnInactiveBg, (s, e) => SetWatchDuration(sVal), 26);
                b.Width = 48;
                b.Padding = new Thickness(0);
                b.Margin = new Thickness(0, 0, 4, 0);
                _watchSecButtons[sVal] = b;
                secRow.Children.Add(b);
            }
            DockPanel.SetDock(secRow, Dock.Left);
            watchBarDock.Children.Add(secRow);

            _btnStartWatch = CreateStyledButton("▶ Start", AccentPrimary, OnStartWatchClick, 26);
            _btnStartWatch.Width = 132;
            _btnStartWatch.Padding = new Thickness(0);
            DockPanel.SetDock(_btnStartWatch, Dock.Right);
            watchBarDock.Children.Add(_btnStartWatch);
            watchBar.Child = watchBarDock;
            DockPanel.SetDock(watchBar, Dock.Top);
            leftDock.Children.Add(watchBar);

            // Manual Override at bottom of left card
            StackPanel manualStack = new StackPanel { Margin = new Thickness(0, 10, 0, 0) };
            DockPanel.SetDock(manualStack, Dock.Bottom);
            _lblManualTitle = CreateThemedText("ResAccentText", 15, FontWeights.Bold);
            _lblManualDesc = CreateThemedText("ResTextSecondary", 11, FontWeights.Normal, new Thickness(0, 0, 0, 8));
            _lblManualDesc.TextWrapping = TextWrapping.Wrap;
            UniformGrid manualBtns = new UniformGrid { Columns = 3 };
            _btnSetInhibit = CreateStyledButton("Inhibit", AccentPrimary, (s, e) => SendOneOffMode("inhibit"), 32);
            _btnSetInhibit.Margin = new Thickness(0, 0, 4, 0);
            _btnSetInhibit.Padding = new Thickness(4, 0, 4, 0);
            _btnSetHold = CreateStyledButton("Hold", ColorWarn, (s, e) => SendOneOffMode("hold"), 32);
            _btnSetHold.Margin = new Thickness(2, 0, 2, 0);
            _btnSetHold.Padding = new Thickness(4, 0, 4, 0);
            _btnSetAuto = CreateStyledButton("Auto", ColorSuccess, (s, e) => SendOneOffMode("auto"), 32);
            _btnSetAuto.Margin = new Thickness(4, 0, 0, 0);
            _btnSetAuto.Padding = new Thickness(4, 0, 4, 0);
            manualBtns.Children.Add(_btnSetInhibit);
            manualBtns.Children.Add(_btnSetHold);
            manualBtns.Children.Add(_btnSetAuto);
            manualStack.Children.Add(CreateHeaderWithInfoBadge(_lblManualTitle, "tip_manual", new Thickness(0, 0, 0, 2)));
            manualStack.Children.Add(_lblManualDesc);
            manualStack.Children.Add(manualBtns);
            leftDock.Children.Add(manualStack);

            // Watch Output Textbox
            _txtWatchOut = CreateCodeTextBox();
            leftDock.Children.Add(_txtWatchOut);

            Grid.SetColumn(leftCard, 0);
            g.Children.Add(leftCard);

            // RIGHT: Diagnostics
            Border rightCard = CreateThemedBorder("ResBgCard", 10, new Thickness(16, 14, 16, 14));
            DockPanel rightDock = new DockPanel();
            rightCard.Child = rightDock;

            DockPanel diagHdr = new DockPanel { Margin = new Thickness(0, 0, 0, 4) };
            _lblDiagTitle = CreateThemedText("ResAccentText", 15, FontWeights.Bold);
            StackPanel diagTitleRow = CreateHeaderWithInfoBadge(_lblDiagTitle, "tip_diag", new Thickness(0));
            _btnRunDiag = CreateStyledButton("Run", AccentPrimary, OnRunDiagnosticsClick, 28);
            _btnRunDiag.Width = 152;
            _btnRunDiag.Padding = new Thickness(0);
            DockPanel.SetDock(diagTitleRow, Dock.Left);
            DockPanel.SetDock(_btnRunDiag, Dock.Right);
            diagHdr.Children.Add(diagTitleRow);
            diagHdr.Children.Add(_btnRunDiag);
            DockPanel.SetDock(diagHdr, Dock.Top);
            rightDock.Children.Add(diagHdr);

            _lblDiagDesc = CreateThemedText("ResTextSecondary", 11, FontWeights.Normal, new Thickness(0, 0, 0, 8));
            _lblDiagDesc.TextWrapping = TextWrapping.Wrap;
            DockPanel.SetDock(_lblDiagDesc, Dock.Top);
            rightDock.Children.Add(_lblDiagDesc);

            _txtDiagOut = CreateCodeTextBox();
            rightDock.Children.Add(_txtDiagOut);

            Grid.SetColumn(rightCard, 2);
            g.Children.Add(rightCard);

            return g;
        }

        // ================= TAB 4: LOGS =================
        private UIElement BuildLogsTab()
        {
            DockPanel dock = new DockPanel();

            Border toolbar = CreateThemedBorder("ResBgCard", 8, new Thickness(14, 8, 14, 8), new Thickness(0, 0, 0, 8));
            DockPanel tbDock = new DockPanel();
            toolbar.Child = tbDock;

            _lblLogTitle = CreateThemedText("ResAccentText", 13.5, FontWeights.Bold);
            _lblLogTitle.VerticalAlignment = VerticalAlignment.Center;
            DockPanel.SetDock(_lblLogTitle, Dock.Left);
            tbDock.Children.Add(_lblLogTitle);

            StackPanel actions = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            DockPanel.SetDock(actions, Dock.Right);

            _lblChkLiveTail = CreateThemedText("ResTextPrimary", 11.5, FontWeights.Normal);
            _chkLiveTail = new CheckBox
            {
                Style = (Style)Resources["SwitchCheckBox"],
                IsChecked = true,
                Content = _lblChkLiveTail,
                Margin = new Thickness(0, 0, 14, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            actions.Children.Add(_chkLiveTail);

            _btnCopyLog = CreateStyledButton("Copy", _btnInactiveBg, OnCopyLogsClick, 28, true);
            _btnCopyLog.Width = 114;
            _btnCopyLog.Padding = new Thickness(0);
            _btnCopyLog.Margin = new Thickness(0, 0, 6, 0);
            actions.Children.Add(_btnCopyLog);

            _btnOpenFolder = CreateStyledButton("Folder", _btnInactiveBg, OnOpenLogFolderClick, 28, true);
            _btnOpenFolder.Width = 96;
            _btnOpenFolder.Padding = new Thickness(0);
            _btnOpenFolder.Margin = new Thickness(0, 0, 6, 0);
            actions.Children.Add(_btnOpenFolder);

            _btnClearLog = CreateStyledButton("Clear", _btnInactiveBg, OnClearLogsClick, 28, true);
            _btnClearLog.Width = 76;
            _btnClearLog.Padding = new Thickness(0);
            actions.Children.Add(_btnClearLog);

            tbDock.Children.Add(actions);
            DockPanel.SetDock(toolbar, Dock.Top);
            dock.Children.Add(toolbar);

            _txtLogs = CreateCodeTextBox();
            _txtLogs.FontSize = 11.5;
            dock.Children.Add(_txtLogs);

            return dock;
        }

        // ================= TAB 5: SYSTEM & PREFERENCES =================
        private UIElement BuildSettingsTab()
        {
            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // LEFT: App Preferences
            Border leftCard = CreateThemedBorder("ResBgCard", 10, new Thickness(18, 16, 18, 16));
            StackPanel leftStack = new StackPanel();
            leftCard.Child = leftStack;

            _lblPrefTitle = CreateThemedText("ResAccentText", 16, FontWeights.Bold, new Thickness(0, 0, 0, 16));
            leftStack.Children.Add(_lblPrefTitle);

            StackPanel trayPrefRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 14), VerticalAlignment = VerticalAlignment.Center };
            _lblSwTray = CreateThemedText("ResTextPrimary", 12.5, FontWeights.Normal);
            _chkMinimizeTray = new CheckBox
            {
                Style = (Style)Resources["SwitchCheckBox"],
                IsChecked = _guiConfig.MinimizeToTray,
                Content = _lblSwTray
            };
            _chkMinimizeTray.Click += (s, e) =>
            {
                _guiConfig.MinimizeToTray = _chkMinimizeTray.IsChecked == true;
                ScheduleConfigSave();
            };
            trayPrefRow.Children.Add(_chkMinimizeTray);
            trayPrefRow.Children.Add(CreateInfoBadgeButton("tip_tray_pref"));
            leftStack.Children.Add(trayPrefRow);

            StackPanel autoPrefRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10), VerticalAlignment = VerticalAlignment.Center };
            _lblSwAutostart = CreateThemedText("ResTextPrimary", 12.5, FontWeights.Normal);
            _chkAutostart = new CheckBox
            {
                Style = (Style)Resources["SwitchCheckBox"],
                IsChecked = Backend.IsAutostartGuiEnabled(),
                Content = _lblSwAutostart
            };
            _chkAutostart.Click += (s, e) =>
            {
                bool en = _chkAutostart.IsChecked == true;
                Backend.SetAutostartGui(en);
                _guiConfig.AutostartGui = en;
                ScheduleConfigSave();
            };
            autoPrefRow.Children.Add(_chkAutostart);
            autoPrefRow.Children.Add(CreateInfoBadgeButton("tip_autostart_pref"));
            leftStack.Children.Add(autoPrefRow);

            _lblAutostartInfo = CreateThemedText("ResTextSecondary", 11.5, FontWeights.Normal, new Thickness(0, 0, 0, 20));
            _lblAutostartInfo.TextWrapping = TextWrapping.Wrap;
            leftStack.Children.Add(_lblAutostartInfo);

            _btnElevate = CreateStyledButton("Elevate", ColorWarn, OnRestartAsAdminClick, 38);
            _btnElevate.Visibility = Backend.IsAdmin() ? Visibility.Collapsed : Visibility.Visible;
            leftStack.Children.Add(_btnElevate);

            Grid.SetColumn(leftCard, 0);
            g.Children.Add(leftCard);

            // RIGHT: Service Management & Full Cleanup
            Border rightCard = CreateThemedBorder("ResBgCard", 10, new Thickness(18, 16, 18, 16));
            DockPanel rightDock = new DockPanel();
            rightCard.Child = rightDock;

            _lblSvcMgrTitle = CreateThemedText("ResAccentText", 16, FontWeights.Bold, new Thickness(0, 0, 0, 12));
            DockPanel.SetDock(_lblSvcMgrTitle, Dock.Top);
            rightDock.Children.Add(_lblSvcMgrTitle);

            Grid svcBtnRow = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            svcBtnRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            svcBtnRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            svcBtnRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            _btnRestartSvc = CreateStyledButton("Restart", AccentPrimary, (s, e) => ControlServiceAsync("restart"), 32);
            _btnRestartSvc.Margin = new Thickness(0, 0, 4, 0);
            Grid.SetColumn(_btnRestartSvc, 0);
            svcBtnRow.Children.Add(_btnRestartSvc);

            _btnStopSvc = CreateStyledButton("Stop", ColorDanger, (s, e) => ControlServiceAsync("stop"), 32);
            _btnStopSvc.Margin = new Thickness(2, 0, 2, 0);
            Grid.SetColumn(_btnStopSvc, 1);
            svcBtnRow.Children.Add(_btnStopSvc);

            _btnStartSvc = CreateStyledButton("Start", ColorSuccess, (s, e) => ControlServiceAsync("start"), 32);
            _btnStartSvc.Margin = new Thickness(4, 0, 0, 0);
            Grid.SetColumn(_btnStartSvc, 2);
            svcBtnRow.Children.Add(_btnStartSvc);

            DockPanel.SetDock(svcBtnRow, Dock.Top);
            rightDock.Children.Add(svcBtnRow);

            _btnUninstallSvc = CreateStyledButton("Uninstall", _btnInactiveBg, OnUninstallServiceClick, 32, true);
            _btnUninstallSvc.Margin = new Thickness(0, 0, 0, 8);
            DockPanel.SetDock(_btnUninstallSvc, Dock.Top);
            rightDock.Children.Add(_btnUninstallSvc);

            _btnFullCleanup = CreateStyledButton("Full Cleanup", ColorDangerDark, OnFullCleanupClick, 36);
            _btnFullCleanup.Margin = new Thickness(0, 0, 0, 12);
            DockPanel.SetDock(_btnFullCleanup, Dock.Top);
            rightDock.Children.Add(_btnFullCleanup);

            // About Card
            Border aboutBox = CreateThemedBorder("ResBgSurface", 8, new Thickness(14, 10, 14, 10));
            StackPanel aboutStack = new StackPanel();
            _lblAboutTitle = CreateThemedText("ResTextPrimary", 12.5, FontWeights.Bold, new Thickness(0, 0, 0, 6));
            _lblAboutText = CreateThemedText("ResTextSecondary", 11.5, FontWeights.Normal);
            _lblAboutText.TextWrapping = TextWrapping.Wrap;
            _lblAboutText.LineHeight = 18;
            aboutStack.Children.Add(_lblAboutTitle);
            aboutStack.Children.Add(_lblAboutText);
            aboutBox.Child = aboutStack;
            rightDock.Children.Add(aboutBox);

            Grid.SetColumn(rightCard, 2);
            g.Children.Add(rightCard);

            return g;
        }

        private TextBox CreateCodeTextBox()
        {
            TextBox tb = new TextBox
            {
                BorderThickness = new Thickness(1),
                FontFamily = new FontFamily("Consolas"),
                FontSize = 11,
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Padding = new Thickness(10)
            };
            tb.SetResourceReference(TextBox.BackgroundProperty, "ResBgCode");
            tb.SetResourceReference(TextBox.ForegroundProperty, "ResAccentText");
            tb.SetResourceReference(TextBox.BorderBrushProperty, "ResBorderSubtle");
            return tb;
        }

        // ================= EVENT HANDLERS & LOGIC =================
        private void ScheduleConfigSave()
        {
            _saveDebounceTimer.Stop();
            _saveDebounceTimer.Start();
        }

        private void SwitchLanguage(string lang)
        {
            I18n.CurrentLang = lang;
            _guiConfig.Language = lang;
            ScheduleConfigSave();
            ApplyTranslations();
            RefreshAllTooltips();
            UpdateSegmentedHighlights();
        }

        private void ApplyPreset(int stop, int start)
        {
            _updatingSliders = true;
            _svcConfig.Stop = stop;
            _svcConfig.Start = start;
            _sliderStop.Value = stop;
            _sliderStart.Value = start;
            _updatingSliders = false;

            _lblStopTitle.Text = I18n.Get("slider_stop_title", stop);
            _lblStartTitle.Text = I18n.Get("slider_start_title", start);
            _guiConfig.LastStop = stop;
            _guiConfig.LastStart = start;
            UpdateSegmentedHighlights();
            ScheduleConfigSave();
        }

        private void OnStopSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_updatingSliders) return;
            _updatingSliders = true;
            int stop = (int)Math.Round(e.NewValue);
            _svcConfig.Stop = stop;
            _lblStopTitle.Text = I18n.Get("slider_stop_title", stop);

            if (_svcConfig.Start >= stop)
            {
                int newStart = Math.Max(40, stop - 5);
                _svcConfig.Start = newStart;
                _sliderStart.Value = newStart;
                _lblStartTitle.Text = I18n.Get("slider_start_title", newStart);
                _guiConfig.LastStart = newStart;
            }
            _guiConfig.LastStop = stop;
            _updatingSliders = false;
            UpdateSegmentedHighlights();
            ScheduleConfigSave();
        }

        private void OnStartSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_updatingSliders) return;
            _updatingSliders = true;
            int start = (int)Math.Round(e.NewValue);
            if (start >= _svcConfig.Stop)
            {
                start = Math.Max(40, _svcConfig.Stop - 5);
                _sliderStart.Value = start;
            }
            _svcConfig.Start = start;
            _lblStartTitle.Text = I18n.Get("slider_start_title", start);
            _guiConfig.LastStart = start;
            _updatingSliders = false;
            UpdateSegmentedHighlights();
            ScheduleConfigSave();
        }

        private void OnOffMinSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_updatingSliders) return;
            int val = (int)Math.Round(e.NewValue);
            _svcConfig.OffMin = val;
            _lblOffMinTitle.Text = I18n.Get("slider_off_min_title", val);
            _guiConfig.LastOffMin = val;
            ScheduleConfigSave();
        }

        private void OnIntervalSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_updatingSliders) return;
            int val = (int)Math.Round(e.NewValue);
            _svcConfig.Interval = val;
            _lblIntervalTitle.Text = I18n.Get("slider_interval_title", val);
            _guiConfig.LastInterval = val;
            ScheduleConfigSave();
        }

        private void SetOffMode(string mode)
        {
            _svcConfig.OffMode = mode;
            _guiConfig.LastOffMode = mode;
            _lblOffModeDesc.Text = I18n.Get("off_mode_" + mode + "_desc");
            UpdateSegmentedHighlights();
            ScheduleConfigSave();
        }

        private void SetRunMode(string mode)
        {
            _svcConfig.RunMode = mode;
            _guiConfig.LastRunMode = mode;
            _lblRunModeDesc.Text = I18n.Get("run_mode_" + mode + "_desc");
            UpdateSegmentedHighlights();
            ScheduleConfigSave();
        }

        private void SetStopMode(string mode)
        {
            _svcConfig.StopMode = mode;
            _guiConfig.LastStopMode = mode;
            _lblStopModeDesc.Text = I18n.Get("stop_mode_" + mode + "_desc");
            UpdateSegmentedHighlights();
            ScheduleConfigSave();
        }

        private void SetWatchDuration(int sec)
        {
            _watchDurationSec = sec;
            UpdateSegmentedHighlights();
        }

        private void UpdateSegmentedHighlights()
        {
            // 1. Secondary buttons
            foreach (Button b in _secondaryButtons)
            {
                b.Background = _btnInactiveBg;
                b.Foreground = _btnInactiveFg;
            }

            // 2. Admin Badge (Standard User adapts to Light/Dark mode)
            bool isAdmin = Backend.IsAdmin();
            _badgeAdminBorder.Background = isAdmin ? ColorSuccess : _btnInactiveBg;
            _lblBadgeAdmin.Foreground = isAdmin ? WhiteBrush : _btnInactiveFg;

            // 3. Navigation Tabs
            for (int i = 0; i < _tabButtons.Count; i++)
                SetButtonActiveState(_tabButtons[i], i == _activeTabIndex);

            // 4. Language Pill
            SetButtonActiveState(_btnLangTr, I18n.CurrentLang == "tr");
            SetButtonActiveState(_btnLangEn, I18n.CurrentLang == "en");

            // 5. Quick Presets
            if (_btnPreset60 != null)
            {
                SetButtonActiveState(_btnPreset60, _svcConfig.Stop == 60 && _svcConfig.Start == 55);
                SetButtonActiveState(_btnPreset80, _svcConfig.Stop == 80 && _svcConfig.Start == 75);
                SetButtonActiveState(_btnPreset90, _svcConfig.Stop == 90 && _svcConfig.Start == 85);
            }

            // 6. Segmented mode buttons
            foreach (var kv in _offModeButtons)
                SetButtonActiveState(kv.Value, kv.Key == _svcConfig.OffMode);
            foreach (var kv in _runModeButtons)
                SetButtonActiveState(kv.Value, kv.Key == _svcConfig.RunMode);
            foreach (var kv in _stopModeButtons)
                SetButtonActiveState(kv.Value, kv.Key == _svcConfig.StopMode);
            foreach (var kv in _watchSecButtons)
                SetButtonActiveState(kv.Value, kv.Key == _watchDurationSec);
        }

        private void OnResetDefaultsClick(object sender, RoutedEventArgs e)
        {
            _updatingSliders = true;
            _svcConfig.Stop = 80;
            _svcConfig.Start = 75;
            _svcConfig.Interval = 30;
            _svcConfig.RunMode = "inhibit";
            _svcConfig.OffMode = "hold";
            _svcConfig.OffMin = 40;
            _svcConfig.StopMode = "auto";

            _sliderStop.Value = 80;
            _sliderStart.Value = 75;
            _sliderOffMin.Value = 40;
            _sliderInterval.Value = 30;
            _updatingSliders = false;

            ApplyTranslations();
            UpdateSegmentedHighlights();
            MessageBox.Show(this, I18n.Get("msg_defaults_loaded"), I18n.Get("msg_info"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnToggleBypassClick(object sender, RoutedEventArgs e)
        {
            if (_updatingBypassToggle) return;
            bool enable = _chkBypass.IsChecked == true;
            _chkBypass.IsEnabled = false;

            Task.Run(() =>
            {
                string msg;
                bool ok = Backend.SetBypass(enable, out msg);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    _chkBypass.IsEnabled = true;
                    if (!ok)
                    {
                        _updatingBypassToggle = true;
                        _chkBypass.IsChecked = !enable;
                        _updatingBypassToggle = false;
                        MessageBox.Show(this, msg, I18n.Get("msg_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    TriggerAsyncRefresh();
                }));
            });
        }

        private void OnSaveSettingsClick(object sender, RoutedEventArgs e)
        {
            _btnSave.IsEnabled = false;
            _btnSave.Content = I18n.Get("btn_saving");

            ServiceConfig copy = new ServiceConfig
            {
                Stop = _svcConfig.Stop,
                Start = _svcConfig.Start,
                Interval = _svcConfig.Interval,
                RunMode = _svcConfig.RunMode,
                OffMode = _svcConfig.OffMode,
                OffMin = _svcConfig.OffMin,
                StopMode = _svcConfig.StopMode
            };

            Task.Run(() =>
            {
                string msg;
                bool ok = Backend.SaveAndApplyServiceConfig(copy, out msg);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    _btnSave.IsEnabled = true;
                    _btnSave.Content = I18n.Get("btn_save");
                    if (ok)
                        MessageBox.Show(this, I18n.Get("msg_saved_service", copy.Stop, copy.Start), I18n.Get("msg_success"), MessageBoxButton.OK, MessageBoxImage.Information);
                    else
                        MessageBox.Show(this, msg, I18n.Get("msg_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                    TriggerAsyncRefresh();
                }));
            });
        }

        private void OnStartWatchClick(object sender, RoutedEventArgs e)
        {
            if (_watchRunning) return;
            _watchRunning = true;
            int secs = _watchDurationSec;
            _btnStartWatch.IsEnabled = false;
            _btnStartWatch.Content = I18n.Get("btn_watching");
            _txtWatchOut.Text = string.Format("Watching battery energy flow for {0} seconds...\r\n", secs);

            Task.Run(() =>
            {
                string so, se;
                Backend.RunCli("status --watch " + secs, out so, out se, false, false);
                string combined = (so ?? "") + (string.IsNullOrEmpty(se) ? "" : "\r\n" + se);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    _txtWatchOut.AppendText(combined + "\r\n");
                    _txtWatchOut.ScrollToEnd();
                    _btnStartWatch.IsEnabled = true;
                    _btnStartWatch.Content = I18n.Get("btn_start_watch");
                    _watchRunning = false;
                }));
            });
        }

        private void SendOneOffMode(string mode)
        {
            Task.Run(() =>
            {
                string so, se;
                int code = Backend.RunCli("set " + mode, out so, out se, true, false);
                string msg = code == 0 ? (string.IsNullOrEmpty(so) ? "Mode set to: " + mode : so) : (string.IsNullOrEmpty(se) ? so : se);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (code == 0)
                        MessageBox.Show(this, msg, I18n.Get("msg_success"), MessageBoxButton.OK, MessageBoxImage.Information);
                    else
                        MessageBox.Show(this, msg, I18n.Get("msg_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                    TriggerAsyncRefresh();
                }));
            });
        }

        private void OnRunDiagnosticsClick(object sender, RoutedEventArgs e)
        {
            _btnRunDiag.IsEnabled = false;
            _txtDiagOut.Text = "Running diagnostics...\r\n";

            Task.Run(() =>
            {
                string so, se;
                Backend.RunCli("diagnose", out so, out se, false, false);
                string combined = (so ?? "") + (string.IsNullOrEmpty(se) ? "" : "\r\n" + se);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    _txtDiagOut.Text = combined;
                    _btnRunDiag.IsEnabled = true;
                }));
            });
        }

        private void RefreshLogsUi(bool force = false)
        {
            if (!force && (_activeTabIndex != 3 || _chkLiveTail.IsChecked != true))
                return;

            Task.Run(() =>
            {
                string content = Backend.ReadLogFile(300);
                if (!force && content == _lastLogContent)
                    return;
                _lastLogContent = content;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    _txtLogs.Text = content;
                    _txtLogs.ScrollToEnd();
                }));
            });
        }

        private void OnCopyLogsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(_txtLogs.Text ?? "");
                MessageBox.Show(this, I18n.Get("msg_copied"), I18n.Get("msg_info"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch { }
        }

        private void OnOpenLogFolderClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Directory.Exists(Backend.ProgramDataDir))
                    Process.Start("explorer.exe", Backend.ProgramDataDir);
            }
            catch { }
        }

        private void OnClearLogsClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(this, I18n.Get("msg_confirm_clear_log"), I18n.Get("msg_warning"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Backend.ClearLogFile();
                RefreshLogsUi(true);
            }
        }

        private void OnRestartAsAdminClick(object sender, RoutedEventArgs e)
        {
            Program.ReleaseMutex();
            Backend.RelaunchAsAdmin();
            ExitApplication();
        }

        private void ControlServiceAsync(string action)
        {
            Task.Run(() =>
            {
                string msg;
                if (action == "restart")
                {
                    Backend.SetServiceState("stop", out msg);
                    Thread.Sleep(900);
                    Backend.SetServiceState("start", out msg);
                }
                else
                {
                    Backend.SetServiceState(action, out msg);
                }
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    MessageBox.Show(this, msg, I18n.Get("msg_info"), MessageBoxButton.OK, MessageBoxImage.Information);
                    TriggerAsyncRefresh();
                }));
            });
        }

        private void OnUninstallServiceClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(this, I18n.Get("msg_uninstall_confirm"), I18n.Get("msg_warning"), MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            Task.Run(() =>
            {
                string msg;
                bool ok = Backend.UninstallService(out msg);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    MessageBox.Show(this, msg, ok ? I18n.Get("msg_success") : I18n.Get("msg_error"), MessageBoxButton.OK, ok ? MessageBoxImage.Information : MessageBoxImage.Error);
                    TriggerAsyncRefresh();
                }));
            });
        }

        private void OnFullCleanupClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(this, I18n.Get("msg_full_cleanup_confirm"), I18n.Get("msg_warning"), MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            _btnFullCleanup.IsEnabled = false;
            Task.Run(() =>
            {
                string msg;
                bool ok = Backend.FullSystemCleanup(out msg);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    _btnFullCleanup.IsEnabled = true;
                    if (ok)
                    {
                        _chkAutostart.IsChecked = false;
                        _lastLogContent = "";
                        MessageBox.Show(this, I18n.Get("msg_full_cleanup_done"), I18n.Get("msg_success"), MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(this, msg, I18n.Get("msg_error"), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    TriggerAsyncRefresh();
                }));
            });
        }

        // ================= TRANSLATIONS & STATUS REFRESH =================
        private void ApplyTranslations()
        {
            _lblMainTitle.Text = I18n.Get("app_title");
            _lblSubTitle.Text = I18n.Get("app_subtitle");
            _lblBadgeAdmin.Text = Backend.IsAdmin() ? I18n.Get("badge_admin") : I18n.Get("badge_user");
            _btnRefresh.Content = I18n.Get("btn_refresh");

            string[] tabKeys = { "tab_status", "tab_advanced", "tab_tools", "tab_logs", "tab_settings" };
            for (int i = 0; i < tabKeys.Length; i++)
                _tabButtons[i].Content = I18n.Get(tabKeys[i]);

            _lblCardBattery.Text = I18n.Get("card_battery_title");
            foreach (var kv in _metricTitles)
                kv.Value.Text = I18n.Get(kv.Key);

            _lblCardLimiter.Text = I18n.Get("card_limiter_title");
            _lblPresets.Text = I18n.Get("lbl_presets");
            _btnPreset60.Content = I18n.Get("preset_60");
            _btnPreset80.Content = I18n.Get("preset_80");
            _btnPreset90.Content = I18n.Get("preset_90");
            _lblStopTitle.Text = I18n.Get("slider_stop_title", _svcConfig.Stop);
            _lblStopDesc.Text = I18n.Get("slider_stop_desc");
            _lblStartTitle.Text = I18n.Get("slider_start_title", _svcConfig.Start);
            _lblStartDesc.Text = I18n.Get("slider_start_desc");
            _lblOffModeTitle.Text = I18n.Get("lbl_off_mode");
            _lblOffModeDesc.Text = I18n.Get("off_mode_" + _svcConfig.OffMode + "_desc");
            _lblBypassSwitch.Text = I18n.Get("switch_bypass");
            _btnSave.Content = I18n.Get("btn_save");

            _lblAdvTitle.Text = I18n.Get("card_adv_title");
            _lblRunModeTitle.Text = I18n.Get("lbl_run_mode");
            _lblRunModeDesc.Text = I18n.Get("run_mode_" + _svcConfig.RunMode + "_desc");
            _lblOffMinTitle.Text = I18n.Get("slider_off_min_title", _svcConfig.OffMin);
            _lblOffMinDesc.Text = I18n.Get("slider_off_min_desc");
            _lblStopModeTitle.Text = I18n.Get("lbl_stop_mode");
            _lblStopModeDesc.Text = I18n.Get("stop_mode_" + _svcConfig.StopMode + "_desc");
            _lblIntervalTitle.Text = I18n.Get("slider_interval_title", _svcConfig.Interval);
            _lblIntervalDesc.Text = I18n.Get("slider_interval_desc");
            _btnResetDefaults.Content = I18n.Get("btn_reset_defaults");

            _lblWatchTitle.Text = I18n.Get("card_watch_title");
            _lblWatchDesc.Text = I18n.Get("card_watch_desc");
            if (!_watchRunning) _btnStartWatch.Content = I18n.Get("btn_start_watch");
            _lblManualTitle.Text = I18n.Get("card_manual_title");
            _lblManualDesc.Text = I18n.Get("card_manual_desc");
            _btnSetInhibit.Content = I18n.Get("btn_set_inhibit");
            _btnSetHold.Content = I18n.Get("btn_set_hold");
            _btnSetAuto.Content = I18n.Get("btn_set_auto");
            _lblDiagTitle.Text = I18n.Get("card_diag_title");
            _lblDiagDesc.Text = I18n.Get("card_diag_desc");
            _btnRunDiag.Content = I18n.Get("btn_run_diag");

            _lblLogTitle.Text = I18n.Get("lbl_log_title");
            _lblChkLiveTail.Text = I18n.Get("chk_live_tail");
            _btnCopyLog.Content = I18n.Get("btn_copy_log");
            _btnOpenFolder.Content = I18n.Get("btn_open_folder");
            _btnClearLog.Content = I18n.Get("btn_clear_log");

            _lblPrefTitle.Text = I18n.Get("card_pref_title");
            _lblSwTray.Text = I18n.Get("sw_tray");
            _lblSwAutostart.Text = I18n.Get("sw_autostart");
            _lblAutostartInfo.Text = I18n.Get("autostart_info");
            _btnElevate.Content = I18n.Get("btn_elevate");
            _lblSvcMgrTitle.Text = I18n.Get("card_service_mgr_title");
            _btnRestartSvc.Content = I18n.Get("btn_restart_svc");
            _btnStopSvc.Content = I18n.Get("btn_stop_svc");
            _btnStartSvc.Content = I18n.Get("btn_start_svc");
            _btnUninstallSvc.Content = I18n.Get("btn_uninstall_svc");
            _btnFullCleanup.Content = I18n.Get("btn_full_cleanup");
            _lblAboutTitle.Text = I18n.Get("card_about_title");
            _lblAboutText.Text = I18n.Get("about_text");

            if (_trayMenuOpen != null)
            {
                _trayMenuOpen.Text = I18n.Get("tray_open");
                _trayMenuBypass.Text = I18n.Get("tray_bypass");
                _trayMenuExit.Text = I18n.Get("tray_exit");
            }

            UpdateBatteryUi(_lastBattery);
        }

        private void TriggerAsyncRefresh()
        {
            Task.Run(() =>
            {
                BatteryStatus st = Backend.GetBatteryStatus();
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    UpdateBatteryUi(st);
                    if (_activeTabIndex == 3 && _chkLiveTail.IsChecked == true)
                        RefreshLogsUi(false);
                }));
            });
        }

        private void UpdateProgressBarWidth(int pct)
        {
            double totalW = _progressTrack.ActualWidth;
            if (totalW > 0)
            {
                double clamped = Math.Max(0, Math.Min(100, pct)) / 100.0;
                _progressFill.Width = totalW * clamped;
            }
        }

        private void UpdateBatteryUi(BatteryStatus st)
        {
            _lastBattery = st;
            _lblBatteryPct.Text = "%" + st.Percent;
            UpdateProgressBarWidth(st.Percent);

            SolidColorBrush accentTextBrush = _isDark ? AccentLightDark : AccentLightMode;
            SolidColorBrush greenTextBrush = _isDark ? ColorSuccessBright : ColorSuccessLightMode;
            SolidColorBrush warnTextBrush = _isDark ? ColorWarnBright : ColorWarnLightMode;
            SolidColorBrush primaryTextBrush = (SolidColorBrush)Resources["ResTextPrimary"];
            SolidColorBrush secTextBrush = (SolidColorBrush)Resources["ResTextSecondary"];

            if (st.State == "Charging")
            {
                _progressFill.Background = ColorSuccessBright;
                _lblBatteryState.Text = I18n.Get("state_charging");
                _lblBatteryState.Foreground = greenTextBrush;
            }
            else if (st.State == "Not charging")
            {
                _progressFill.Background = AccentPrimary;
                _lblBatteryState.Text = I18n.Get("state_inhibited");
                _lblBatteryState.Foreground = accentTextBrush;
            }
            else if (st.State == "Discharging")
            {
                _progressFill.Background = ColorWarnBright;
                _lblBatteryState.Text = I18n.Get("state_discharging");
                _lblBatteryState.Foreground = warnTextBrush;
            }
            else
            {
                _progressFill.Background = _btnInactiveBg;
                _lblBatteryState.Text = I18n.Get("state_unknown");
                _lblBatteryState.Foreground = secTextBrush;
            }

            _metricValues["ac"].Text = (st.Ac == "online") ? I18n.Get("metric_ac_online") : I18n.Get("metric_ac_offline");
            _metricValues["ac"].Foreground = (st.Ac == "online") ? greenTextBrush : warnTextBrush;
            _metricValues["power"].Text = st.Power;
            _metricValues["energy"].Text = st.Energy;
            _metricValues["voltage"].Text = st.Voltage;
            _metricValues["health"].Text = st.Health;
            _metricValues["cycles"].Text = st.Cycles;
            _metricValues["model"].Text = st.PackModel;
            _metricValues["bypass"].Text = st.Bypass ? I18n.Get("metric_bypass_active") : I18n.Get("metric_bypass_inactive");
            _metricValues["bypass"].Foreground = st.Bypass ? warnTextBrush : primaryTextBrush;
            _metricValues["firmware"].Text = st.Firmware;
            _metricValues["firmware"].Foreground = st.Firmware.Contains("inhibit") ? accentTextBrush : (st.Firmware.Contains("hold") ? warnTextBrush : primaryTextBrush);
            _metricValues["last_off"].Text = st.LastOff;

            _badgeSvcBorder.Background = st.ServiceActive ? ColorSuccess : ColorDanger;
            _lblBadgeSvc.Text = st.ServiceActive ? I18n.Get("badge_svc_running") : I18n.Get("badge_svc_stopped");

            if (_chkBypass.IsChecked != st.Bypass)
            {
                _updatingBypassToggle = true;
                _chkBypass.IsChecked = st.Bypass;
                _updatingBypassToggle = false;
            }

            if (_trayIcon != null)
            {
                _trayIcon.Text = string.Format("HP Charge Limiter: %{0} ({1})", st.Percent, st.State);
            }
        }

        // ================= SYSTEM TRAY =================
        private void SetupSystemTray()
        {
            _trayIcon = new Forms.NotifyIcon
            {
                Text = "HP Battery Charge Limiter",
                Visible = true
            };
            LoadCrispTrayIcon();

            Forms.ContextMenuStrip menu = new Forms.ContextMenuStrip();
            _trayMenuOpen = new Forms.ToolStripMenuItem(I18n.Get("tray_open"), null, (s, e) => RestoreFromTray());
            _trayMenuBypass = new Forms.ToolStripMenuItem(I18n.Get("tray_bypass"), null, (s, e) =>
            {
                bool next = !_lastBattery.Bypass;
                Task.Run(() =>
                {
                    string msg;
                    Backend.SetBypass(next, out msg);
                    Dispatcher.BeginInvoke(new Action(TriggerAsyncRefresh));
                });
            });
            _trayMenuExit = new Forms.ToolStripMenuItem(I18n.Get("tray_exit"), null, (s, e) => ExitApplication());

            menu.Items.Add(_trayMenuOpen);
            menu.Items.Add(_trayMenuBypass);
            menu.Items.Add(new Forms.ToolStripSeparator());
            menu.Items.Add(_trayMenuExit);

            _trayIcon.ContextMenuStrip = menu;
            _trayIcon.DoubleClick += (s, e) => RestoreFromTray();
        }

        private void LoadCrispTrayIcon()
        {
            try
            {
                System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
                using (Stream iconStream = asm.GetManifestResourceStream("HPChargeLimiter.app_icon.ico"))
                {
                    if (iconStream != null)
                    {
                        _trayIcon.Icon = new Drawing.Icon(iconStream, new Drawing.Size(32, 32));
                        return;
                    }
                }
            }
            catch { }

            try
            {
                _trayIcon.Icon = Drawing.Icon.ExtractAssociatedIcon(
                    System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            catch { }
        }

        public void RestoreFromTray()
        {
            Show();
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;
            Activate();
            Focus();
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isExiting && _guiConfig.MinimizeToTray)
            {
                e.Cancel = true;
                Hide();
                try
                {
                    _trayIcon.ShowBalloonTip(2000, "HP Charge Limiter", I18n.Get("toast_minimized"), Forms.ToolTipIcon.Info);
                }
                catch { }
            }
            else
            {
                CleanupTray();
            }
        }

        private void CleanupTray()
        {
            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
                _trayIcon = null;
            }
        }

        public void ExitApplication()
        {
            _isExiting = true;
            Backend.SaveGuiConfig(_guiConfig);
            CleanupTray();
            Program.ReleaseMutex();
            Application.Current.Shutdown();
        }
    }
}
