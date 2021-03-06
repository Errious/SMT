﻿using SMT.EVEData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SMT
{
    /// <summary>
    /// Interaction logic for RegionControl.xaml
    /// </summary>
    public partial class RegionControl : UserControl, INotifyPropertyChanged
    {
        public EVEData.MapRegion Region { get; set; }
        public MapConfig MapConf { get; set; }
        public EveManager EM { get; set; }
        public AnomManager ANOMManager { get; set; }
        public string SelectedSystem { get; set; }

        // Constant Colours
        private Brush StandingVBadBrush  = new SolidColorBrush(Color.FromArgb(110, 148, 5, 5));
        private Brush StandingBadBrush   = new SolidColorBrush(Color.FromArgb(110, 196, 72, 6));
        private Brush StandingNeutBrush  = new SolidColorBrush(Color.FromArgb(110, 140, 140, 140));
        private Brush StandingGoodBrush  = new SolidColorBrush(Color.FromArgb(110, 43, 101, 196));
        private Brush StandingVGoodBrush = new SolidColorBrush(Color.FromArgb(110, 5, 34, 120));



        private LocalCharacter m_ActiveCharacter;
        public LocalCharacter ActiveCharacter
        {
            get
            {
                return m_ActiveCharacter;
            }
            set
            {
                m_ActiveCharacter = value;
                OnPropertyChanged("ActiveCharacter");
            }
        }


        // Store the Dynamic Map elements so they can seperately be cleared
        private List<System.Windows.UIElement> DynamicMapElements;

        // Timer to Re-draw the map
        private System.Windows.Threading.DispatcherTimer uiRefreshTimer;


        // Map Controls
        private double m_ESIOverlayScale = 1.0f;
        private bool m_ShowNPCKills = false;
        private bool m_ShowPodKills = false;
        private bool m_ShowShipKills = false;
        private bool m_ShowShipJumps = false;
        private bool m_ShowJumpBridges = true;
        private bool m_ShowStandings = false;
        private bool m_ShowSov = false;
        private bool m_ShowSystemSecurity = false;

        private List<Point> SystemIcon_Keepstar = new List<Point>
        {
            new Point(1,17),
            new Point(1,0),
            new Point(7,0),
            new Point(7,7),
            new Point(12,7),
            new Point(12,0),
            new Point(18,0),
            new Point(18,17),
        };

        private List<Point> SystemIcon_Fortizar = new List<Point>
        {
            new Point(4,12),
            new Point(4,7),
            new Point(6,7),
            new Point(6,5),
            new Point(12,5),
            new Point(12,7),
            new Point(14,7),
            new Point(14,12),
        };

        private List<Point> SystemIcon_Astrahaus = new List<Point>
        {
            new Point(6,12),
            new Point(6,7),
            new Point(9,7),
            new Point(9,4),
            //new Point(10,4),
            new Point(9,7),
            new Point(12,7),
            new Point(12,12),
        };

        private List<Point> SystemIcon_NPCStation = new List<Point>
        {
            new Point(2,16),
            new Point(2,2),
            new Point(16,2),
            new Point(16,16),
            

        };


        public bool ShowStandings
        {
            get
            {
                return m_ShowStandings;
            }
            set
            {
                m_ShowStandings = value;
                OnPropertyChanged("ShowStandings");
            }
        }

        public bool ShowSov
        {
            get
            {
                return m_ShowSov;
            }
            set
            {
                m_ShowSov = value;
                OnPropertyChanged("ShowSov");
            }
        }


        public bool ShowSystemSecurity
        {
            get
            {
                return m_ShowSystemSecurity;
            }
            set
            {
                m_ShowSystemSecurity = value;
                OnPropertyChanged("ShowSystemSecurity");
            }
        }




        public bool ShowJumpBridges
        {
            get
            {
                return m_ShowJumpBridges;
            }
            set
            {
                m_ShowJumpBridges = value;
                OnPropertyChanged("ShowJumpBridges");
            }
        }


        public double ESIOverlayScale
        {
            get
            {
                return m_ESIOverlayScale;
            }
            set
            {
                m_ESIOverlayScale = value;
                OnPropertyChanged("ESIOverlayScale");
            }
        }


        public bool ShowNPCKills
        {
            get
            {
                return m_ShowNPCKills;
            }

            set
            {
                m_ShowNPCKills = value;

                if (m_ShowNPCKills)
                {
                    ShowPodKills = false;
                    ShowShipKills = false;
                    ShowShipJumps = false;
                }

                OnPropertyChanged("ShowNPCKills");
            }
        }

        public bool ShowPodKills
        {
            get
            {
                return m_ShowPodKills;
            }

            set
            {
                m_ShowPodKills = value;
                if (m_ShowPodKills)
                {
                    ShowNPCKills = false;
                    ShowShipKills = false;
                    ShowShipJumps = false;
                }

                OnPropertyChanged("ShowPodKills");
            }
        }

        public bool ShowShipKills
        {
            get
            {
                return m_ShowShipKills;
            }

            set
            {
                m_ShowShipKills = value;
                if (m_ShowShipKills)
                {
                    ShowNPCKills = false;
                    ShowPodKills = false;
                    ShowShipJumps = false;
                }

                OnPropertyChanged("ShowShipKills");
            }
        }

        public bool ShowShipJumps
        {
            get
            {
                return m_ShowShipJumps;
            }

            set
            {
                m_ShowShipJumps = value;
                if (m_ShowShipJumps)
                {
                    ShowNPCKills = false;
                    ShowPodKills = false;
                    ShowShipKills = false;
                }

                OnPropertyChanged("ShowShipJumps");
            }
        }

        public bool FollowCharacter
        {
            get
            {
                return FollowCharacterChk.IsChecked.Value;
            }
            set
            {
                FollowCharacterChk.IsChecked = value;
            }
        }





        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public event PropertyChangedEventHandler RegionChanged;

        protected void OnRegionChanged(string name)
        {
            PropertyChangedEventHandler handler = RegionChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }


        public event PropertyChangedEventHandler CharacterSelectionChanged;

        protected void OnCharacterSelectionChanged(string name)
        {
            PropertyChangedEventHandler handler = CharacterSelectionChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }




        /// <summary>
        /// Redraw the map
        /// </summary>
        /// <param name="FullRedraw">Clear all the static items or not</param>
        public void ReDrawMap(bool FullRedraw = false)
        {
            if (ActiveCharacter != null && FollowCharacter == true)
            { 
                HandleCharacterSelectionChange();
            }



            if (FullRedraw)
            {
                // reset the background
                MainCanvasGrid.Background = new SolidColorBrush(MapConf.ActiveColourScheme.MapBackgroundColour);
                MainCanvas.Background = new SolidColorBrush(MapConf.ActiveColourScheme.MapBackgroundColour);
                MainZoomControl.Background = new SolidColorBrush(MapConf.ActiveColourScheme.MapBackgroundColour);

                MainCanvas.Children.Clear();

                // re-add the static content
                AddSystemsToMap();
            }
            else
            {
                // remove anything temporary
                foreach (UIElement uie in DynamicMapElements)
                {
                    MainCanvas.Children.Remove(uie);
                }
                DynamicMapElements.Clear();
            }

            AddCharactersToMap();
            AddDataToMap();
            AddSystemIntelOverlay();
            AddHighlightToSystem(SelectedSystem);
            AddRouteToMap();
            AddTheraSystemsToMap();
            AddEveTraceFleetsToMap();
        }


        private struct GateHelper
        {
            public EVEData.MapSystem from { get; set; }
            public EVEData.MapSystem to { get; set; }
        }


        private const double SYSTEM_SHAPE_SIZE = 18;
        private const double SYSTEM_SHAPE_OFFSET = SYSTEM_SHAPE_SIZE / 2;
        private const double SYSTEM_TEXT_TEXT_SIZE= 6;
        private const double SYSTEM_TEXT_X_OFFSET = 10;
        private const double SYSTEM_TEXT_Y_OFFSET = 2;
        private const double SYSTEM_REGION_TEXT_X_OFFSET = 5;
        private const double SYSTEM_REGION_TEXT_Y_OFFSET = SYSTEM_TEXT_Y_OFFSET + SYSTEM_TEXT_TEXT_SIZE + 2;

        private const int SYSTEM_Z_INDEX = 22;
        private const int SYSTEM_LINK_INDEX = 19;

        /// <summary>
        /// Initialise the control
        /// </summary>
        public void Init()
        {
            EM = EVEData.EveManager.Instance;
            SelectedSystem = string.Empty;
            BridgeInfoL1.Content = string.Empty;
            BridgeInfoL2.Content = string.Empty;


            DynamicMapElements = new List<UIElement>();

            CharacterDropDown.ItemsSource = EM.LocalCharacters;
            ActiveCharacter = null;

            RegionSelectCB.ItemsSource = EM.Regions;
            SelectRegion(MapConf.DefaultRegion);


            uiRefreshTimer = new System.Windows.Threading.DispatcherTimer();
            uiRefreshTimer.Tick += UiRefreshTimer_Tick; ;
            uiRefreshTimer.Interval = new TimeSpan(0, 0, 2);
            uiRefreshTimer.Start();

            DataContext = this;

            //ToolBoxCanvas.DataContext = this;

            PropertyChanged += MapObjectChanged;
        }

        /// <summary>
        /// UI Refresh Timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UiRefreshTimer_Tick(object sender, EventArgs e)
        {

            if (MapConf.CurrentJumpCharacter != "")
            {
                foreach (LocalCharacter c in EM.LocalCharacters)
                {
                    if (c.Name == MapConf.CurrentJumpCharacter)
                    {
                        MapConf.CurrentJumpSystem = c.Location;
                    }
                }
            }

            ReDrawMap(false);
        }

        public void SelectSystem(string name, bool changeRegion = false)
        {
            if(SelectedSystem == name)
            {
                return;
            }

            EVEData.System sys = EM.GetEveSystem(name);

            if(sys == null)
            {
                return;
            }

            if(changeRegion && !Region.IsSystemOnMap(name))
            {
                SelectRegion(sys.Region);
            }


            foreach (EVEData.MapSystem es in Region.MapSystems.Values.ToList())
            {
                if (es.Name == name)
                {
                    SystemDropDownAC.SelectedItem = es;
                    SelectedSystem = es.Name;
                    AddHighlightToSystem(name);
                    if(!MapConf.LockJumpSystem)
                    {
                        MapConf.CurrentJumpSystem = es.Name;
                        MapConf.CurrentJumpCharacter = "";
                        BridgeInfoL1.Content = MapConf.JumpShipType + " range from";
                        BridgeInfoL2.Content = es.Name;

                        MapConf.CurrentJumpCharacter = "";
                    }


                    break;
                }
            }


            // now setup the anom data


            EVEData.AnomData system = ANOMManager.GetSystemAnomData(name);
            ANOMManager.ActiveSystem = system;

            ///AnomSigList.ItemsSource = system.Anoms.Values;


        }



        /// <summary>
        /// Select A Region
        /// </summary>
        /// <param name="regionName">Region to Select</param>
        public void SelectRegion(string regionName)
        {
            // check we havent selected the same system
            if(Region !=null && Region.Name == regionName)
            {
                return;
            }

            EM.UpdateIDsForMapRegion(regionName);

            // check its a valid system
            EVEData.MapRegion mr = EM.GetRegion(regionName);
            if(mr == null)
            {
                return;
            }

            // update the selected region
            Region = mr;
            RegionNameLabel.Content = mr.Name;
            MapConf.DefaultRegion = mr.Name;

            List<EVEData.MapSystem> newList = Region.MapSystems.Values.ToList().OrderBy(o => o.Name).ToList();
            SystemDropDownAC.ItemsSource = newList;

// SJS Disabled until ticket resolved with CCP
//            if (ActiveCharacter != null)
//            {
//                ActiveCharacter.UpdateStructureInfoForRegion(regionName);
//            }


            ReDrawMap(true);

            // select the item in the dropdown
            RegionSelectCB.SelectedItem = Region;

            OnRegionChanged(regionName);

        }

        /// <summary>
        /// Add the base systems, and jumps to the map
        /// </summary>
        private void AddSystemsToMap()
        {
            // brushes
            Brush SysOutlineBrush = new SolidColorBrush(MapConf.ActiveColourScheme.SystemOutlineColour);
            Brush SysInRegionBrush = new SolidColorBrush(MapConf.ActiveColourScheme.InRegionSystemColour);
            Brush SysOutRegionBrush = new SolidColorBrush(MapConf.ActiveColourScheme.OutRegionSystemColour);

            Brush SysInRegionDarkBrush = new SolidColorBrush(DarkenColour(MapConf.ActiveColourScheme.InRegionSystemColour));
            Brush SysOutRegionDarkBrush = new SolidColorBrush(DarkenColour(MapConf.ActiveColourScheme.OutRegionSystemColour));



            Brush SysInRegionTextBrush = new SolidColorBrush(MapConf.ActiveColourScheme.InRegionSystemTextColour);
            Brush SysOutRegionTextBrush = new SolidColorBrush(MapConf.ActiveColourScheme.OutRegionSystemTextColour);

            Brush FriendlyJumpBridgeBrush = new SolidColorBrush(MapConf.ActiveColourScheme.FriendlyJumpBridgeColour);
            Brush HostileJumpBridgeBrush = new SolidColorBrush(MapConf.ActiveColourScheme.HostileJumpBridgeColour);

            Brush JumpInRange = new SolidColorBrush(MapConf.ActiveColourScheme.JumpRangeInColour);

            Brush Incursion = new SolidColorBrush(MapConf.ActiveColourScheme.ActiveIncursionColour);


            //HatchBrush  Incursion = new HatchBrush(HatchStyle.DiagonalCross, System.Drawing.Color.FromArgb(MapConf.ActiveColourScheme.ActiveIncursionColour.GetHashCode()));
                //MapConf.ActiveColourScheme.ActiveIncursionColour);

            Color bgtc = MapConf.ActiveColourScheme.MapBackgroundColour;
            bgtc.A = 192;
            Brush SysTextBackgroundBrush = new SolidColorBrush(bgtc);

            Brush NormalGateBrush = new SolidColorBrush(MapConf.ActiveColourScheme.NormalGateColour);
            Brush ConstellationGateBrush = new SolidColorBrush(MapConf.ActiveColourScheme.ConstellationGateColour);

            // cache all system links 
            List<GateHelper> systemLinks = new List<GateHelper>();

            Random rnd = new Random(4);



            foreach (EVEData.MapSystem system in Region.MapSystems.Values.ToList())
            {
                // add circle for system
                Polygon systemShape = new Polygon();
                systemShape.StrokeThickness = 1.5;
                
                bool needsOutline = true;
                bool drawKeep = false;
                bool drawFort = false;
                bool drawAstra = false;
                bool drawNPCStation = system.ActualSystem.HasNPCStation;

                foreach(StructureHunter.Structures sh in system.ActualSystem.SHStructures)
                {
                    switch (sh.TypeId)
                    {
                        case 35834: // Keepstar
                            drawKeep = true;
                            break;

                        case 35833: // fortizar 
                        case 47512: // faction fortizar 
                        case 47513: // faction fortizar 
                        case 47514: // faction fortizar 
                        case 47515: // faction fortizar 
                        case 47516: // faction fortizar
                        case 35827: // Sotiyo
                            drawFort = true;
                            break;

                        default:
                            drawAstra = true;
                            break;

                    }

                }

                /*if(ActiveCharacter != null && ActiveCharacter.ESILinked && ActiveCharacter.DockableStructures.Keys.Contains(system.Name))
                {
                    foreach(StructureIDs.StructureIdData sid in ActiveCharacter.DockableStructures[system.Name])
                    {
                        switch(sid.TypeId)
                        {
                            case 35834: // Keepstar
                                drawKeep = true;
                                break;

                            case 35833: // fortizar 
                            case 47512: // faction fortizar 
                            case 47513: // faction fortizar 
                            case 47514: // faction fortizar 
                            case 47515: // faction fortizar 
                            case 47516: // faction fortizar
                            case 35827: // Sotiyo
                                drawFort = true;
                                break;

                            default:
                                drawAstra = true;
                                break;

                        }
                    }
                }
                */

                if (drawKeep)
                {
                    drawFort = false;
                    drawAstra = false;
                    drawNPCStation = false;

                    needsOutline = false;
                }

                if(drawFort)
                {
                    drawAstra = false;
                    drawNPCStation = false;
                }

                if(drawNPCStation)
                {
                    drawAstra = false;
                }

                List<Point> shapePoints = null;
                
                if(drawKeep)
                {
                    shapePoints = SystemIcon_Keepstar;
                    systemShape.StrokeThickness = 1.5;
                }
                    
                if(drawFort)
                {
                    shapePoints = SystemIcon_Fortizar;
                    systemShape.StrokeThickness = 1;

                }
                if (drawAstra)
                {
                    shapePoints = SystemIcon_Astrahaus;
                    systemShape.StrokeThickness = 1;
                }

                if (drawNPCStation)
                {
                    shapePoints = SystemIcon_NPCStation;
                    systemShape.StrokeThickness = 1.5;
                    needsOutline = false;
                }

                if(shapePoints != null)
                {
                    foreach (Point p in shapePoints)
                    {
                        systemShape.Points.Add(p);
                    }



                    systemShape.Stroke = SysOutlineBrush;
                    systemShape.StrokeLineJoin = PenLineJoin.Round;

                    if (system.OutOfRegion)
                    {
                        systemShape.Fill = SysOutRegionDarkBrush;
                    }
                    else
                    {
                        systemShape.Fill = SysInRegionDarkBrush;
                    }

                    // override with sec status colours
                    if (ShowSystemSecurity)
                    {
                        systemShape.Fill = new SolidColorBrush(MapColours.GetSecStatusColour(system.ActualSystem.TrueSec, MapConf.ShowTrueSec));
                    }

                    if (!needsOutline)
                    {
                        // add the hover over and click handlers
                        systemShape.DataContext = system;
                        systemShape.MouseDown += ShapeMouseDownHandler;
                        systemShape.MouseEnter += ShapeMouseOverHandler;
                        systemShape.MouseLeave += ShapeMouseOverHandler;
                    }
                    else
                    {
                        systemShape.IsHitTestVisible = false;
                    }

                    Canvas.SetLeft(systemShape, system.LayoutX - SYSTEM_SHAPE_OFFSET);
                    Canvas.SetTop(systemShape, system.LayoutY - SYSTEM_SHAPE_OFFSET);
                    Canvas.SetZIndex(systemShape, SYSTEM_Z_INDEX);
                    MainCanvas.Children.Add(systemShape);

                }




                if (needsOutline)
                {
                    Shape SystemOutline = new Ellipse { Width = SYSTEM_SHAPE_SIZE, Height = SYSTEM_SHAPE_SIZE };
                    SystemOutline.Stroke = SysOutlineBrush;
                    SystemOutline.StrokeThickness = 1.5;
                    SystemOutline.StrokeLineJoin = PenLineJoin.Round;

                    if (system.OutOfRegion)
                    {
                        SystemOutline.Fill = SysOutRegionBrush;
                    }
                    else
                    {
                        SystemOutline.Fill = SysInRegionBrush;
                    }

                    // override with sec status colours
                    if (ShowSystemSecurity)
                    {
                        SystemOutline.Fill = new SolidColorBrush(MapColours.GetSecStatusColour(system.ActualSystem.TrueSec, MapConf.ShowTrueSec));
                    }

                    SystemOutline.DataContext = system;
                    SystemOutline.MouseDown += ShapeMouseDownHandler;
                    SystemOutline.MouseEnter += ShapeMouseOverHandler;
                    SystemOutline.MouseLeave += ShapeMouseOverHandler;

                    Canvas.SetLeft(SystemOutline, system.LayoutX - SYSTEM_SHAPE_OFFSET);
                    Canvas.SetTop(SystemOutline, system.LayoutY - SYSTEM_SHAPE_OFFSET);
                    Canvas.SetZIndex(SystemOutline, SYSTEM_Z_INDEX - 1);
                    MainCanvas.Children.Add(SystemOutline);
                }

                Label sysText = new Label();
                sysText.Content = system.Name;

                if (MapConf.ActiveColourScheme.SystemTextSize > 0)
                {
                    sysText.FontSize = MapConf.ActiveColourScheme.SystemTextSize;
                }

                if (system.OutOfRegion)
                {
                    sysText.Foreground = SysOutRegionTextBrush;
                }
                else
                {
                    sysText.Foreground = SysInRegionTextBrush;
                }
                Thickness border = new Thickness(0.0);

                sysText.Padding = border;
                sysText.Margin = border;

                Canvas.SetLeft(sysText, system.LayoutX + SYSTEM_TEXT_X_OFFSET);
                Canvas.SetTop(sysText, system.LayoutY + SYSTEM_TEXT_Y_OFFSET);
                Canvas.SetZIndex(sysText, SYSTEM_Z_INDEX);

                MainCanvas.Children.Add(sysText);

                // generate the list of links
                foreach (string jumpTo in system.ActualSystem.Jumps)
                {
                    if (Region.IsSystemOnMap(jumpTo))
                    {
                        EVEData.MapSystem to = Region.MapSystems[jumpTo];

                        bool NeedsAdd = true;
                        foreach (GateHelper gh in systemLinks)
                        {
                            if (((gh.from == system) || (gh.to == system)) && ((gh.from == to) || (gh.to == to)))
                            {
                                NeedsAdd = false;
                                break;
                            }
                        }

                        if (NeedsAdd)
                        {
                            GateHelper g = new GateHelper();
                            g.from = system;
                            g.to = to;
                            systemLinks.Add(g);
                        }
                    }
                }

                double regionMarkerOffset = SYSTEM_REGION_TEXT_Y_OFFSET ;

                if (MapConf.ShowActiveIncursions && system.ActualSystem.ActiveIncursion)
                {
                    {
                        Polygon poly = new Polygon();

                        foreach (Point p in system.CellPoints)
                        {
                            poly.Points.Add(p);
                        }

                        //poly.Fill
                        poly.Fill = Incursion;
                        poly.SnapsToDevicePixels = true;
                        poly.Stroke = poly.Fill;
                        poly.StrokeThickness = 3;
                        poly.StrokeDashCap = PenLineCap.Round;
                        poly.StrokeLineJoin = PenLineJoin.Round;
                        MainCanvas.Children.Add(poly);
                    }
                }

                if ((ShowSov) && system.ActualSystem.SOVAlliance != null && EM.AllianceIDToName.Keys.Contains(system.ActualSystem.SOVAlliance))
                {
                    Label sysRegionText = new Label();

                    string content = "";
                    string allianceName = EM.GetAllianceName(system.ActualSystem.SOVAlliance);
                    string allianceTicker = EM.GetAllianceTicker(system.ActualSystem.SOVAlliance);
                    content = allianceTicker;

                    sysRegionText.Content = content;
                    sysRegionText.FontSize = SYSTEM_TEXT_TEXT_SIZE;
                    sysText.FontSize = MapConf.ActiveColourScheme.SystemTextSize;

                    Canvas.SetLeft(sysRegionText, system.LayoutX + SYSTEM_REGION_TEXT_X_OFFSET);
                    Canvas.SetTop (sysRegionText, system.LayoutY + SYSTEM_REGION_TEXT_Y_OFFSET);
                    Canvas.SetZIndex(sysRegionText, SYSTEM_Z_INDEX);

                    MainCanvas.Children.Add(sysRegionText);

                    regionMarkerOffset += SYSTEM_TEXT_TEXT_SIZE;
                }

                if(!MapConf.ShowJumpDistance)
                {
                    BridgeInfoL1.Content = string.Empty;
                    BridgeInfoL2.Content = string.Empty;
                }



 

                if (MapConf.ShowJumpDistance && MapConf.CurrentJumpSystem != null && system.Name != MapConf.CurrentJumpSystem)
                {

                    double Distance = EM.GetRangeBetweenSystems(MapConf.CurrentJumpSystem, system.Name);
                    Distance = Distance / 9460730472580800.0;

                    double Max = 0.1f;

                    switch (MapConf.JumpShipType)
                    {
                        case MapConfig.JumpShip.Super: { Max = 6.0; } break;
                        case MapConfig.JumpShip.Titan: { Max = 6.0; } break;

                        case MapConfig.JumpShip.Dread: { Max = 7.0; } break;
                        case MapConfig.JumpShip.Carrier: { Max = 7.0; } break;
                        case MapConfig.JumpShip.FAX: { Max = 7.0; } break;
                        case MapConfig.JumpShip.Blops: { Max = 8.0; } break;
                        case MapConfig.JumpShip.Rorqual: { Max = 10.0; } break;
                        case MapConfig.JumpShip.JF: { Max = 10.0; } break;
                    }

                    EVEData.System js = EM.GetEveSystem(MapConf.CurrentJumpSystem);

                    if (MapConf.CurrentJumpCharacter != "")
                    {
                        BridgeInfoL1.Content = MapConf.JumpShipType + " range from";
                        BridgeInfoL2.Content = MapConf.CurrentJumpCharacter + " : " + MapConf.CurrentJumpSystem + " (" + js.Region + ")";
                    }
                    else
                    {
                        BridgeInfoL1.Content = MapConf.JumpShipType + " range from";
                        BridgeInfoL2.Content = MapConf.CurrentJumpSystem + " (" + js.Region + ")";
                    }


                    if (Distance < Max && Distance > 0.0)
                    {
                        string JD = Distance.ToString("0.00") + " LY";

                        Label DistanceText = new Label();

                        DistanceText.Content = JD;
                        DistanceText.FontSize = 9;
                        regionMarkerOffset += 8;

                        Canvas.SetLeft(DistanceText, system.LayoutX + SYSTEM_REGION_TEXT_X_OFFSET);
                        Canvas.SetTop(DistanceText, system.LayoutY + SYSTEM_REGION_TEXT_Y_OFFSET);


                        Canvas.SetZIndex(DistanceText, 20);
                        MainCanvas.Children.Add(DistanceText);

                        if (MapConf.JumpRangeInAsOutline)
                        {
                            Shape InRangeMarker;

                            if (system.ActualSystem.HasNPCStation)
                            {
                                InRangeMarker = new Rectangle() { Height = SYSTEM_SHAPE_SIZE + 6, Width = SYSTEM_SHAPE_SIZE + 6 };
                            }
                            else
                            {
                                InRangeMarker = new Ellipse() { Height = SYSTEM_SHAPE_SIZE + 6, Width = SYSTEM_SHAPE_SIZE + 6 };
                            }

                            InRangeMarker.Stroke = JumpInRange;
                            InRangeMarker.StrokeThickness = 6;
                            InRangeMarker.StrokeLineJoin = PenLineJoin.Round;
                            InRangeMarker.Fill = JumpInRange;

                            Canvas.SetLeft(InRangeMarker, system.LayoutX - (SYSTEM_SHAPE_SIZE + 6) / 2);
                            Canvas.SetTop(InRangeMarker, system.LayoutY - (SYSTEM_SHAPE_SIZE + 6) / 2);
                            Canvas.SetZIndex(InRangeMarker, 19);


                            MainCanvas.Children.Add(InRangeMarker);
                        }
                        else
                        {
                            Polygon poly = new Polygon();

                            foreach (Point p in system.CellPoints)
                            {
                                poly.Points.Add(p);
                            }

                            poly.Fill = JumpInRange;
                            poly.SnapsToDevicePixels = true;
                            poly.Stroke = poly.Fill;
                            poly.StrokeThickness = 3;
                            poly.StrokeDashCap = PenLineCap.Round;
                            poly.StrokeLineJoin = PenLineJoin.Round;
                            MainCanvas.Children.Add(poly);
                        }
                    }
                }


                if (system.OutOfRegion)
                {
                    Label sysRegionText = new Label();
                    sysRegionText.Content = "(" + system.Region + ")";
                    sysRegionText.FontSize = SYSTEM_TEXT_TEXT_SIZE;
                    sysRegionText.Foreground = new SolidColorBrush(MapConf.ActiveColourScheme.OutRegionSystemTextColour);

                    Canvas.SetLeft(sysRegionText, system.LayoutX + SYSTEM_REGION_TEXT_X_OFFSET);
                    Canvas.SetTop(sysRegionText, system.LayoutY + regionMarkerOffset);
                    Canvas.SetZIndex(sysRegionText, SYSTEM_Z_INDEX);

                    MainCanvas.Children.Add(sysRegionText);
                }
            }

            // now add the links
            foreach (GateHelper gh in systemLinks)
            {
                Line sysLink = new Line();

                sysLink.X1 = gh.from.LayoutX;
                sysLink.Y1 = gh.from.LayoutY;

                sysLink.X2 = gh.to.LayoutX;
                sysLink.Y2 = gh.to.LayoutY;

                if (gh.from.ActualSystem.Region != gh.to.ActualSystem.Region || gh.from.ActualSystem.ConstellationID != gh.to.ActualSystem.ConstellationID)
                {
                    sysLink.Stroke = ConstellationGateBrush;
                }
                else
                {
                    sysLink.Stroke = NormalGateBrush;
                }

                sysLink.StrokeThickness = 1.2;
                sysLink.Visibility = Visibility.Visible;

                Canvas.SetZIndex(sysLink, SYSTEM_LINK_INDEX);
                MainCanvas.Children.Add(sysLink);
            }

            if (ShowJumpBridges && EM.JumpBridges != null)
            {
                foreach (EVEData.JumpBridge jb in EM.JumpBridges)
                {
                    if (Region.IsSystemOnMap(jb.From) || Region.IsSystemOnMap(jb.To))
                    {
                        EVEData.MapSystem from;

                        if (!Region.IsSystemOnMap(jb.From))
                        {
                            from = Region.MapSystems[jb.To];
                        }
                        else
                        {
                            from = Region.MapSystems[jb.From];
                        }

                        Point startPoint = new Point(from.LayoutX, from.LayoutY);
                        Point endPoint;
                        
                        if (!Region.IsSystemOnMap(jb.To) || !Region.IsSystemOnMap(jb.From))
                        {
                            endPoint = new Point(from.LayoutX - 20, from.LayoutY - 40);

                            Shape jbOutofSystemBlob = new Ellipse() { Height = 6, Width = 6 };
                            Canvas.SetLeft(jbOutofSystemBlob, endPoint.X - 3 );
                            Canvas.SetTop(jbOutofSystemBlob, endPoint.Y - 3 );
                            Canvas.SetZIndex(jbOutofSystemBlob, 19);

                            MainCanvas.Children.Add(jbOutofSystemBlob);


                            if (jb.Friendly)
                            {
                                jbOutofSystemBlob.Stroke = new SolidColorBrush(MapConf.ActiveColourScheme.FriendlyJumpBridgeColour);
                                jbOutofSystemBlob.Fill = jbOutofSystemBlob.Stroke;
                            }
                            else
                            {
                                jbOutofSystemBlob.Stroke = new SolidColorBrush(MapConf.ActiveColourScheme.HostileJumpBridgeColour);
                                jbOutofSystemBlob.Fill = jbOutofSystemBlob.Stroke;
                            }


                        }
                        else
                        {
                            EVEData.MapSystem to = Region.MapSystems[jb.To];
                            endPoint = new Point(to.LayoutX, to.LayoutY);
                        }

                        Vector dir = Point.Subtract(startPoint, endPoint);

                        double jbDistance = Point.Subtract(startPoint, endPoint).Length;

                        Size arcSize = new Size(jbDistance + 60, jbDistance + 60);

                        ArcSegment arcseg = new ArcSegment(endPoint, arcSize, 140, false, SweepDirection.Clockwise, true);

                        PathSegmentCollection pscollection = new PathSegmentCollection();
                        pscollection.Add(arcseg);

                        PathFigure pf = new PathFigure();
                        pf.Segments = pscollection;
                        pf.StartPoint = startPoint;

                        PathFigureCollection pfcollection = new PathFigureCollection();
                        pfcollection.Add(pf);

                        PathGeometry pathGeometry = new PathGeometry();
                        pathGeometry.Figures = pfcollection;

                        System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();
                        path.Data = pathGeometry;

                        if (jb.Friendly)
                        {
                            path.Stroke = new SolidColorBrush(MapConf.ActiveColourScheme.FriendlyJumpBridgeColour);
                        }
                        else
                        {
                            path.Stroke = new SolidColorBrush(MapConf.ActiveColourScheme.HostileJumpBridgeColour);
                        }

                        path.StrokeThickness = 2;

                        DoubleCollection dashes = new DoubleCollection();
                        dashes.Add(1.0);
                        dashes.Add(1.0);

                        path.StrokeDashArray = dashes;

                        // animate the jump bridges
                        DoubleAnimation da = new DoubleAnimation();
                        da.From = 0;
                        da.To = 200;
                        da.By = 2;
                        da.Duration = new Duration(TimeSpan.FromSeconds(90));
                        da.RepeatBehavior = RepeatBehavior.Forever;

                        path.StrokeDashArray = dashes;
                        path.BeginAnimation(Shape.StrokeDashOffsetProperty, da);

                        Canvas.SetZIndex(path, 19);

                        MainCanvas.Children.Add(path);
                    }
                }
            }
        }

        private Color DarkenColour(Color inCol)
        {
            Color Dark = inCol;
            Dark.R = (Byte)(0.8 * Dark.R);
            Dark.G = (Byte)(0.8 * Dark.G);
            Dark.B = (Byte)(0.8 * Dark.B);
            return Dark;
        }

        private void AddHighlightToSystem(string name)
        {
            if (!Region.MapSystems.Keys.Contains(name))
            {
                return;
            }

            EVEData.MapSystem selectedSys = Region.MapSystems[name];
            if (selectedSys != null)
            {
                double circleSize = 30;
                double circleOffset = circleSize / 2;

                // add circle for system
                Shape highlightSystemCircle = new Ellipse() { Height = circleSize, Width = circleSize };
                highlightSystemCircle.Stroke = new SolidColorBrush(MapConf.ActiveColourScheme.SelectedSystemColour);

                highlightSystemCircle.StrokeThickness = 3;

                RotateTransform rt = new RotateTransform();
                rt.CenterX = circleSize / 2;
                rt.CenterY = circleSize / 2;
                highlightSystemCircle.RenderTransform = rt;

                DoubleCollection dashes = new DoubleCollection();
                dashes.Add(1.0);
                dashes.Add(1.0);

                highlightSystemCircle.StrokeDashArray = dashes;

                Canvas.SetLeft(highlightSystemCircle, selectedSys.LayoutX - circleOffset);
                Canvas.SetTop(highlightSystemCircle, selectedSys.LayoutY - circleOffset);
                Canvas.SetZIndex(highlightSystemCircle, 19);

                MainCanvas.Children.Add(highlightSystemCircle);
                DynamicMapElements.Add(highlightSystemCircle);

                DoubleAnimation da = new DoubleAnimation();
                da.From = 0;
                da.To = 360;
                da.Duration = new Duration(TimeSpan.FromSeconds(12));

                try
                {
                    RotateTransform eTransform = (RotateTransform)highlightSystemCircle.RenderTransform;
                    eTransform.BeginAnimation(RotateTransform.AngleProperty, da);
                }
                catch
                {

                }
            }
        }


        /// <summary>
        /// Add Characters to the region
        /// </summary>
        void AddCharactersToMap()
        {
            // Cache all characters in the same system so we can render them on seperate lines
            Dictionary<string, List<EVEData.LocalCharacter>> charLocationMap = new Dictionary<string, List<EVEData.LocalCharacter>>();

            foreach (EVEData.LocalCharacter c in EM.LocalCharacters)
            {
                // ignore characters out of this Map..
                if (!Region.IsSystemOnMap(c.Location))
                {
                    continue;
                }


                if (!charLocationMap.Keys.Contains(c.Location))
                {
                    charLocationMap[c.Location] = new List<EVEData.LocalCharacter>();
                }
                charLocationMap[c.Location].Add(c);
            }
            
            foreach (List<EVEData.LocalCharacter> lc in charLocationMap.Values)
            {
                double textYOffset = -24;
                double textXOffset = 6;

                EVEData.MapSystem ms = Region.MapSystems[lc[0].Location];

                // add circle for system

                double circleSize = 26;
                double circleOffset = circleSize / 2;

                Shape highlightSystemCircle = new Ellipse() { Height = circleSize, Width = circleSize };

                highlightSystemCircle.Stroke = new SolidColorBrush(MapConf.ActiveColourScheme.CharacterHighlightColour);
                highlightSystemCircle.StrokeThickness = 3;

                RotateTransform rt = new RotateTransform();
                rt.CenterX = circleSize / 2;
                rt.CenterY = circleSize / 2;
                highlightSystemCircle.RenderTransform = rt;

                DoubleCollection dashes = new DoubleCollection();
                dashes.Add(1.0);
                dashes.Add(1.0);

                highlightSystemCircle.StrokeDashArray = dashes;

                Canvas.SetLeft(highlightSystemCircle, ms.LayoutX - circleOffset);
                Canvas.SetTop(highlightSystemCircle, ms.LayoutY - circleOffset);
                Canvas.SetZIndex(highlightSystemCircle, 25);

                MainCanvas.Children.Add(highlightSystemCircle);
                DynamicMapElements.Add(highlightSystemCircle);

                // Storyboard s = new Storyboard();
                DoubleAnimation da = new DoubleAnimation();
                da.From = 360;
                da.To = 0;
                da.Duration = new Duration(TimeSpan.FromSeconds(12));
                da.RepeatBehavior = RepeatBehavior.Forever;

                RotateTransform eTransform = (RotateTransform)highlightSystemCircle.RenderTransform;
                eTransform.BeginAnimation(RotateTransform.AngleProperty, da);
                
                foreach (EVEData.LocalCharacter c in lc)
                {
                    Label charText = new Label();
                    charText.Content = c.Name;
                    charText.Foreground = new SolidColorBrush(MapConf.ActiveColourScheme.CharacterTextColour);
                    charText.IsHitTestVisible = false;

                    if (MapConf.ActiveColourScheme.CharacterTextSize > 0)
                    {
                        charText.FontSize = MapConf.ActiveColourScheme.CharacterTextSize;
                    }

                    Canvas.SetLeft(charText, ms.LayoutX + textXOffset);
                    Canvas.SetTop(charText, ms.LayoutY + textYOffset);
                    Canvas.SetZIndex(charText, 20);
                    MainCanvas.Children.Add(charText);
                    DynamicMapElements.Add(charText);

                    textYOffset -= (MapConf.ActiveColourScheme.CharacterTextSize + 4);
                }
            }
        }

        private void AddSystemIntelOverlay()
        {
            Brush intelBlobBrush = new SolidColorBrush(MapConf.ActiveColourScheme.IntelOverlayColour);
            foreach (EVEData.IntelData id in EM.IntelDataList)
            {
                foreach (string sysStr in id.Systems)
                {
                    if (Region.IsSystemOnMap(sysStr))
                    {
                        EVEData.MapSystem sys = Region.MapSystems[sysStr];

                        double radiusScale = (DateTime.Now - id.IntelTime).TotalSeconds / (double)MapConf.MaxIntelSeconds;

                        if (radiusScale < 0.0 || radiusScale >= 1.0)
                        {
                            continue;
                        }

                        // add circle to the map
                        double radius = 24 + (100 * (1.0 - radiusScale));
                        double circleOffset = radius / 2;

                        Shape intelShape = new Ellipse() { Height = radius, Width = radius };

                        intelShape.Fill = intelBlobBrush;
                        Canvas.SetLeft(intelShape, sys.LayoutX - circleOffset);
                        Canvas.SetTop(intelShape, sys.LayoutY - circleOffset);
                        Canvas.SetZIndex(intelShape, 15);
                        MainCanvas.Children.Add(intelShape);

                        DynamicMapElements.Add(intelShape);
                    }
                }
            }
        }

        private void AddRouteToMap()
        {


            if (ActiveCharacter == null)
                return;

            Brush RouteBrush = new SolidColorBrush(Colors.Yellow);
            Brush WaypointBrush = new SolidColorBrush(Colors.DarkGray);

            // no active route
            if (ActiveCharacter.ActiveRoute.Count == 0)
            {
                return;
            }

            string Start = "";
            string End = ActiveCharacter.Location;

            for (int i = 0; i < ActiveCharacter.ActiveRoute.Count; i++)
            {
                Start = End;
                End = ActiveCharacter.ActiveRoute[i];

                if (!(Region.IsSystemOnMap(Start) && Region.IsSystemOnMap(End)))
                {
                    continue;
                }

                EVEData.MapSystem from = Region.MapSystems[Start];
                EVEData.MapSystem to = Region.MapSystems[End];


                Line routeLine = new Line();


                routeLine.X1 = from.LayoutX;
                routeLine.Y1 = from.LayoutY;

                routeLine.X2 = to.LayoutX;
                routeLine.Y2 = to.LayoutY;

                routeLine.StrokeThickness = 5;
                routeLine.Visibility = Visibility.Visible;
                routeLine.Stroke = RouteBrush;

                DoubleCollection dashes = new DoubleCollection();
                dashes.Add(1.0);
                dashes.Add(1.0);

                routeLine.StrokeDashArray = dashes;

                // animate the jump bridges
                DoubleAnimation da = new DoubleAnimation();
                da.From = 200;
                da.To = 0;
                da.By = 2;
                da.Duration = new Duration(TimeSpan.FromSeconds(40));
                da.RepeatBehavior = RepeatBehavior.Forever;

                routeLine.StrokeDashArray = dashes;
                routeLine.BeginAnimation(Shape.StrokeDashOffsetProperty, da);



                Canvas.SetZIndex(routeLine, 18);
                MainCanvas.Children.Add(routeLine);

                DynamicMapElements.Add(routeLine);
            }
        }


        public void AddEveTraceFleetsToMap()
        {
            Brush TheraBrush = new SolidColorBrush(Colors.OrangeRed);

            if(EM.FleetIntel == null)
            {
                return;
            }

            foreach( EveTrace.FleetInstance fi in EM.FleetIntel.FleetInstances)
            {
                EVEData.System s = EM.GetEveSystemFromID(fi.System.ToString());
                if(s==null)
                {
                    continue;
                }

                if(Region.IsSystemOnMap(s.Name))
                {
                    int size = (int)(fi.KillCount * ESIOverlayScale);

                    Shape ETShape = new Rectangle() { Height = SYSTEM_SHAPE_SIZE + size, Width = SYSTEM_SHAPE_SIZE + size }; ;
                    ETShape.Stroke = TheraBrush;
                    ETShape.StrokeThickness = 1.5;
                    ETShape.StrokeLineJoin = PenLineJoin.Round;
                    ETShape.Fill = TheraBrush;

                    MapSystem ms = Region.MapSystems[s.Name];

                    Canvas.SetLeft(ETShape, ms.LayoutX - (ETShape.Width/2));
                    Canvas.SetTop(ETShape, ms.LayoutY - (ETShape.Height/2));
                    Canvas.SetZIndex(ETShape, SYSTEM_Z_INDEX - 3);
                    MainCanvas.Children.Add(ETShape);

                }
            }
        }

        public void AddTheraSystemsToMap()
        {
            Brush TheraBrush = new SolidColorBrush(Colors.YellowGreen);

            foreach(TheraConnection tc in EM.TheraConnections)
            {
                if(Region.IsSystemOnMap(tc.System))
                {
                    MapSystem ms = Region.MapSystems[tc.System];

                    Shape TheraShape;
                    if (ms.ActualSystem.HasNPCStation)
                    {
                        TheraShape = new Rectangle() { Height = SYSTEM_SHAPE_SIZE+6, Width = SYSTEM_SHAPE_SIZE+6 };
                    }
                    else
                    {
                        TheraShape = new Ellipse() { Height = SYSTEM_SHAPE_SIZE+6, Width = SYSTEM_SHAPE_SIZE+6 };
                    }

                    TheraShape.Stroke = TheraBrush;
                    TheraShape.StrokeThickness = 1.5;
                    TheraShape.StrokeLineJoin = PenLineJoin.Round;
                    TheraShape.Fill = TheraBrush;

                    // add the hover over and click handlers


                    Canvas.SetLeft(TheraShape, ms.LayoutX - (SYSTEM_SHAPE_OFFSET +3 ));
                    Canvas.SetTop(TheraShape, ms.LayoutY - (SYSTEM_SHAPE_OFFSET +3));
                    Canvas.SetZIndex(TheraShape, SYSTEM_Z_INDEX -3);
                    MainCanvas.Children.Add(TheraShape);
                }
            }
        }


        public Color stringToColour(string str)
        {
            int hash = 0;

            foreach (char c in str.ToCharArray())
            {
                hash = c + ((hash << 5) - hash);
            }

            double R = (((byte)(hash & 0xff) / 255.0) * 80.0) + 127.0;
            double G = (((byte)((hash >> 8) & 0xff) / 255.0) * 80.0) + 127.0;
            double B = (((byte)((hash >> 16) & 0xff) / 255.0) * 80.0) + 127.0;

            return Color.FromArgb(100, (byte)R, (byte)G, (byte)B);
        }

        private void AddDataToMap()
        {
            Color DataColor = MapConf.ActiveColourScheme.ESIOverlayColour;
            Color DataLargeColor = MapConf.ActiveColourScheme.ESIOverlayColour;

            DataLargeColor.R = (byte)(DataLargeColor.R * 0.75);
            DataLargeColor.G = (byte)(DataLargeColor.G * 0.75);
            DataLargeColor.B = (byte)(DataLargeColor.B * 0.75);


            SolidColorBrush infoColour = new SolidColorBrush(DataColor);
            SolidColorBrush zkbColour = new SolidColorBrush(Colors.Purple);

            SolidColorBrush infoLargeColour = new SolidColorBrush(DataLargeColor);

            foreach (EVEData.MapSystem sys in Region.MapSystems.Values.ToList())
            {
                int nPCKillsLastHour = sys.ActualSystem.NPCKillsLastHour;
                int podKillsLastHour = sys.ActualSystem.PodKillsLastHour;
                int shipKillsLastHour = sys.ActualSystem.ShipKillsLastHour;
                int jumpsLastHour = sys.ActualSystem.JumpsLastHour;

                int infoValue = -1;
                double infoSize = 0.0;

                if (ShowNPCKills)
                {
                    infoValue = nPCKillsLastHour;
                    infoSize = 0.15f * infoValue * ESIOverlayScale;
                }

                if (ShowPodKills)
                {
                    infoValue = podKillsLastHour;
                    infoSize = 20.0f * infoValue * ESIOverlayScale;
                }

                if (ShowShipKills)
                {
                    infoValue = shipKillsLastHour;
                    infoSize = 20.0f * infoValue * ESIOverlayScale;
                }

                if (ShowShipJumps)
                {
                    infoValue = sys.ActualSystem.JumpsLastHour;
                    infoSize = infoValue * ESIOverlayScale;
                }


                if (infoValue > 0)
                {
                    // clamp to a minimum
                    if (infoSize < 24)
                        infoSize = 24;


                    Shape infoCircle = new Ellipse() { Height = infoSize, Width = infoSize };
                    infoCircle.Fill = infoColour;

                    Canvas.SetZIndex(infoCircle, 10);
                    Canvas.SetLeft(infoCircle, sys.LayoutX - (infoSize / 2));
                    Canvas.SetTop(infoCircle, sys.LayoutY - (infoSize / 2));
                    MainCanvas.Children.Add(infoCircle);
                    DynamicMapElements.Add(infoCircle);
                }

                if(infoSize > 60)
                {
                    Shape infoCircle = new Ellipse() { Height = 30, Width = 30 };
                    infoCircle.Fill = infoLargeColour;

                    Canvas.SetZIndex(infoCircle, 11);
                    Canvas.SetLeft(infoCircle, sys.LayoutX - (15));
                    Canvas.SetTop(infoCircle, sys.LayoutY - (15));
                    MainCanvas.Children.Add(infoCircle);
                    DynamicMapElements.Add(infoCircle);

                }

                if ( sys.ActualSystem.SOVAlliance != null && ShowStandings)
                {
                    Polygon poly = new Polygon();

                    foreach (Point p in sys.CellPoints)
                    {
                        poly.Points.Add(p);
                    }

                    bool addToMap = true;
                    Brush br = null;
                    
                    if (ActiveCharacter != null && ActiveCharacter.ESILinked)
                    {
                        float Standing = 0.0f;

                        if (ActiveCharacter.AllianceID != null && ActiveCharacter.AllianceID == sys.ActualSystem.SOVAlliance)
                        {
                            Standing = 10.0f;
                        }

                        if (sys.ActualSystem.SOVCorp != null && ActiveCharacter.Standings.Keys.Contains(sys.ActualSystem.SOVCorp))
                        {
                            Standing = ActiveCharacter.Standings[sys.ActualSystem.SOVCorp];
                        }

                        if (sys.ActualSystem.SOVAlliance != null && ActiveCharacter.Standings.Keys.Contains(sys.ActualSystem.SOVAlliance))
                        {
                            Standing = ActiveCharacter.Standings[sys.ActualSystem.SOVAlliance];
                        }

                        if (Standing == 0.0f)
                        {
                            addToMap = false;
                        }

                        br = StandingNeutBrush;

                        if (Standing == -10.0)
                        {
                            br = StandingVBadBrush;
                        }

                        if (Standing == -5.0)
                        {
                            br = StandingBadBrush;
                        }

                        if (Standing == 5.0)
                        {
                            br = StandingGoodBrush;
                        }

                        if (Standing == 10.0)
                        {
                            br = StandingVGoodBrush;
                        }
                    }
                    else
                    {
                        // enabled but not linked
                        addToMap = false;
                    }


                    poly.Fill = br;
                    poly.SnapsToDevicePixels = true;
                    poly.Stroke = poly.Fill;
                    poly.StrokeThickness = 0.5;
                    poly.StrokeDashCap = PenLineCap.Round;
                    poly.StrokeLineJoin = PenLineJoin.Round;

                    if (addToMap)
                    {
                        MainCanvas.Children.Add(poly);

                        // save the dynamic map elements
                        DynamicMapElements.Add(poly);
                    }
                }
            }

            Dictionary<string, int> ZKBBaseFeed = new Dictionary<string, int>();
            {
                foreach (EVEData.ZKillRedisQ.ZKBDataSimple zs in EM.ZKillFeed.KillStream)
                {
                    if (ZKBBaseFeed.Keys.Contains(zs.SystemName))
                    {
                        ZKBBaseFeed[zs.SystemName]++;
                    }
                    else
                    {
                        ZKBBaseFeed[zs.SystemName] = 1;
                    }
                }


                foreach (EVEData.MapSystem sys in Region.MapSystems.Values.ToList())
                {
                    if (ZKBBaseFeed.Keys.Contains(sys.ActualSystem.Name))
                    {
                        double ZKBValue = 24 + ((double)ZKBBaseFeed[sys.ActualSystem.Name] * ESIOverlayScale * 2);

                        Shape infoCircle = new Ellipse() { Height = ZKBValue, Width = ZKBValue };
                        infoCircle.Fill = zkbColour;

                        Canvas.SetZIndex(infoCircle, 11);
                        Canvas.SetLeft(infoCircle, sys.LayoutX - (ZKBValue / 2));
                        Canvas.SetTop(infoCircle, sys.LayoutY - (ZKBValue / 2));
                        MainCanvas.Children.Add(infoCircle);
                        DynamicMapElements.Add(infoCircle);
                    }
                }
            }

        }

        /// <summary>
        /// Constructor
        /// </summary>
        public RegionControl()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Region Selection Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegionSelectCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FollowCharacter = false;

            EVEData.MapRegion rd = RegionSelectCB.SelectedItem as EVEData.MapRegion;
            if(rd == null)
            {
                return;
            }

            SelectRegion(rd.Name);
        }

        /// <summary>
        /// Add Waypoint Clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SysContexMenuItemAddWaypoint_Click(object sender, RoutedEventArgs e)
        {
            EVEData.MapSystem eveSys = ((System.Windows.FrameworkElement)((System.Windows.FrameworkElement)sender).Parent).DataContext as EVEData.MapSystem;
            if (ActiveCharacter != null)
            {
                ActiveCharacter.AddDestination(eveSys.ActualSystem.ID, false);
            }

        }

        /// <summary>
        /// Set Destination Clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SysContexMenuItemSetDestination_Click(object sender, RoutedEventArgs e)
        {
            EVEData.MapSystem eveSys = ((System.Windows.FrameworkElement)((System.Windows.FrameworkElement)sender).Parent).DataContext as EVEData.MapSystem;
            if (ActiveCharacter != null)
            {
                ActiveCharacter.AddDestination(eveSys.ActualSystem.ID, true);
            }
        }

        /// <summary>
        /// Copy Clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SysContexMenuItemCopy_Click(object sender, RoutedEventArgs e)
        {
            EVEData.MapSystem eveSys = ((System.Windows.FrameworkElement)((System.Windows.FrameworkElement)sender).Parent).DataContext as EVEData.MapSystem;

            try
            {
                if (eveSys != null)
                {
                    Clipboard.SetText(eveSys.Name);
                }
            }
            catch { }
        }

        /// <summary>
        /// Dotlan Clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SysContexMenuItemDotlan_Click(object sender, RoutedEventArgs e)
        {
            EVEData.MapSystem eveSys = ((System.Windows.FrameworkElement)((System.Windows.FrameworkElement)sender).Parent).DataContext as EVEData.MapSystem;
            EVEData.MapRegion rd = EM.GetRegion(eveSys.Region);

            string uRL = string.Format("http://evemaps.dotlan.net/map/{0}/{1}", rd.DotLanRef, eveSys.Name);
            System.Diagnostics.Process.Start(uRL);
        }

        /// <summary>
        /// ZKillboard Clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SysContexMenuItemZKB_Click(object sender, RoutedEventArgs e)
        {
            EVEData.MapSystem eveSys = ((System.Windows.FrameworkElement)((System.Windows.FrameworkElement)sender).Parent).DataContext as EVEData.MapSystem;
            EVEData.MapRegion rd = EM.GetRegion(eveSys.Region);

            string uRL = string.Format("https://zkillboard.com/system/{0}", eveSys.ActualSystem.ID);
            System.Diagnostics.Process.Start(uRL);
        }


        /// <summary>
        /// Shape (ie System) MouseDown handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShapeMouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            Shape obj = sender as Shape;

            EVEData.MapSystem selectedSys = obj.DataContext as EVEData.MapSystem;

            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 1)
                {
                    bool redraw = false;
                    if (MapConf.ShowJumpDistance)
                    {
                        redraw = true;
                    }
                    FollowCharacter = false;
                    SelectSystem(selectedSys.Name);

                    ReDrawMap(redraw);

                }

                if (e.ClickCount == 2 && selectedSys.Region != Region.Name)
                {
                    foreach (EVEData.MapRegion rd in EM.Regions)
                    {
                        if (rd.Name == selectedSys.Region)
                        {
                            RegionSelectCB.SelectedItem = rd;

                            ReDrawMap();
                            SelectSystem(selectedSys.Name);
                            break;
                        }
                    }
                }
            }

            if (e.ChangedButton == MouseButton.Right)
            {
                ContextMenu cm = this.FindResource("SysRightClickContextMenu") as ContextMenu;
                cm.PlacementTarget = obj;
                cm.DataContext = selectedSys;

                MenuItem setDesto = cm.Items[2] as MenuItem;
                MenuItem addWaypoint = cm.Items[3] as MenuItem;


                setDesto.IsEnabled = false;
                addWaypoint.IsEnabled = false;


                if (ActiveCharacter != null && ActiveCharacter.ESILinked)
                {
                    setDesto.IsEnabled = true;
                    addWaypoint.IsEnabled = true;
                }



                cm.IsOpen = true;
            }
        }

        /// <summary>
        /// Shape (ie System) Mouse over handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShapeMouseOverHandler(object sender, MouseEventArgs e)
        {
            Shape obj = sender as Shape;

            EVEData.MapSystem selectedSys = obj.DataContext as EVEData.MapSystem;

            if (obj.IsMouseOver && MapConf.ShowSystemPopup)
            {
                SystemInfoPopup.PlacementTarget = obj;
                SystemInfoPopup.VerticalOffset = 5;
                SystemInfoPopup.HorizontalOffset = 15;
                SystemInfoPopup.DataContext = selectedSys.ActualSystem;


                // check JB Info
                SystemInfoPopup_JBInfo.Content = "";
                if(ShowJumpBridges)
                {
                    foreach (EVEData.JumpBridge jb in EM.JumpBridges)
                    {
                        if (selectedSys.Name == jb.From)
                        {
                            SystemInfoPopup_JBInfo.Content = "JB (" + jb.FromInfo + ") to " + jb.To;
                        }

                        if (selectedSys.Name == jb.To)
                        {
                            SystemInfoPopup_JBInfo.Content = "JB (" + jb.ToInfo + ") to " + jb.From;
                        }
                    }
                }
                SystemInfoPopup.IsOpen = true;
            }
            else
            {
                SystemInfoPopup.IsOpen = false;
            }
        }

        private void HandleCharacterSelectionChange()
        {
            EVEData.LocalCharacter c = CharacterDropDown.SelectedItem as EVEData.LocalCharacter;
            if(ActiveCharacter != c)
            {
                ActiveCharacter = c;
                OnCharacterSelectionChanged(c.Name);
            }


            if (c != null && FollowCharacter)
            {
                EVEData.System s = EM.GetEveSystem(c.Location);
                if (s != null)
                {
                    if (s.Region != Region.Name)
                    {
                        // change region
                        SelectRegion(s.Region);
                    }

                    SelectSystem(c.Location);

                    CharacterDropDown.SelectedItem = c;

                    // force the follow as this will be reset by the region change
                    FollowCharacter = true;
                }
            }
        }

        private void CharacterDropDown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HandleCharacterSelectionChange();
        }

        private void CharacterDropDown_DropDownClosed(object sender, EventArgs e)
        {
            HandleCharacterSelectionChange();
        }

        private void FollowCharacterChk_Checked(object sender, RoutedEventArgs e)
        {
            HandleCharacterSelectionChange();
        }

        private void SystemDropDownAC_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EVEData.MapSystem sd = SystemDropDownAC.SelectedItem as EVEData.MapSystem;

            if (sd != null)
            {
                SelectSystem(sd.Name);
                ReDrawMap(false);
            }
        }

        private void MapObjectChanged(object sender, PropertyChangedEventArgs e)
        {
            ReDrawMap(true);
        }
    }
}
