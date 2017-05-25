using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing;
using GTA;
using GTA.Native;
using Control = GTA.Control;

namespace SimpleUI
{
    class MenuPool
    {
        List<UIMenu> _menuList = new List<UIMenu>();
        public UIMenu LastUsedMenu { get; set; }

        public void AddMenu(UIMenu menu)
        {
            _menuList.Add(menu);
        }

        /// <summary>
        /// Adds a submenu to a parent menu and to the MenuPool.
        /// </summary>
        /// <param name="SubMenu">The submenu</param>
        /// <param name="ParentMenu">The parent menu.</param>
        /// <param name="text">The text of the menu item in the parent menu that leads to the submenu when entered.</param>
        public void AddSubMenu(UIMenu SubMenu, UIMenu ParentMenu, string text, bool UseSameColorsAsParent = true)
        {
            AddMenu(SubMenu);
            /*SubMenu.ParentMenu = ParentMenu;
            ParentMenu.NextMenu = SubMenu;*/
            var item = new UIMenuItem(text);
            //ParentMenu.BindingMenuItem = BindingItem;
            ParentMenu.AddMenuItem(item);
            //ParentMenu.BindingMenuItem = item;
            ParentMenu.BindItemToSubmenu(SubMenu, item);

            if (UseSameColorsAsParent)
            {
                SubMenu.TitleColor = ParentMenu.TitleColor;
                SubMenu.TitleUnderlineColor = ParentMenu.TitleUnderlineColor;
                SubMenu.TitleBackgroundColor = ParentMenu.TitleBackgroundColor;

                SubMenu.DefaultTextColor = ParentMenu.DefaultTextColor;
                SubMenu.DefaultBoxColor = ParentMenu.DefaultBoxColor;
                SubMenu.HighlightedItemTextColor = ParentMenu.HighlightedItemTextColor;
                SubMenu.HighlightedBoxColor = ParentMenu.HighlightedBoxColor;

                SubMenu.DescriptionTextColor = ParentMenu.DescriptionTextColor;
                SubMenu.DescriptionBoxColor = ParentMenu.DescriptionBoxColor;
            }
        }

        /// <summary>
        /// Adds a submenu to a parent menu and to the MenuPool.
        /// </summary>
        /// <param name="SubMenu">The submenu</param>
        /// <param name="ParentMenu">The parent menu.</param>
        /// <param name="text">The text of the menu item in the parent menu that leads to the submenu when entered.</param>
        /// <param name="description">The description of the menu item that leads to the submenu when entered.</param>
        public void AddSubMenu(UIMenu SubMenu, UIMenu ParentMenu, string text, string description, bool UseSameColorsAsParent = true)
        {
            AddMenu(SubMenu);
            //SubMenu.ParentMenu = ParentMenu;
            //ParentMenu.NextMenu = SubMenu;
            var item = new UIMenuItem(text, null, description);
            //ParentMenu.BindingMenuItem = BindingItem;
            ParentMenu.AddMenuItem(item);
            //ParentMenu.BindingMenuItem = item;
            ParentMenu.BindItemToSubmenu(SubMenu, item);

            if (UseSameColorsAsParent)
            {
                SubMenu.TitleColor = ParentMenu.TitleColor;
                SubMenu.TitleUnderlineColor = ParentMenu.TitleUnderlineColor;
                SubMenu.TitleBackgroundColor = ParentMenu.TitleBackgroundColor;

                SubMenu.DefaultTextColor = ParentMenu.DefaultTextColor;
                SubMenu.DefaultBoxColor = ParentMenu.DefaultBoxColor;
                SubMenu.HighlightedItemTextColor = ParentMenu.HighlightedItemTextColor;
                SubMenu.HighlightedBoxColor = ParentMenu.HighlightedBoxColor;

                SubMenu.DescriptionTextColor = ParentMenu.DescriptionTextColor;
                SubMenu.DescriptionBoxColor = ParentMenu.DescriptionBoxColor;
            }
        }

        /// <summary>
        /// Draws all visible menus.
        /// </summary>
        public void Draw()
        {
            foreach (var menu in _menuList.Where(menu => menu.IsVisible))
            {
                menu.Draw();
                SetLastUsedMenu(menu);
            }
        }

        /// <summary>
        /// Set the last used menu.
        /// </summary>
        public void SetLastUsedMenu(UIMenu menu)
        {
            LastUsedMenu = menu;
        }

        /// <summary>
        /// Process all of your menus' functions. Call this in a tick event.
        /// </summary>
        public void ProcessMenus()
        {
            if (LastUsedMenu == null)
            {
                LastUsedMenu = _menuList[0];
            }
            Draw();
        }

        /// <summary>
        /// Checks if any menu is currently visible.
        /// </summary>
        /// <returns>true if at least one menu is visible, false if not.</returns>
        public bool IsAnyMenuOpen()
        {
            return _menuList.Any(menu => menu.IsVisible);
        }

        /// <summary>
        /// Closes all of your menus.
        /// </summary>
        public void CloseAllMenus()
        {
            foreach (var menu in _menuList.Where(menu => menu.IsVisible))
            {
                menu.IsVisible = false;
            }
        }

        public void RemoveAllMenus()
        {
            _menuList.Clear();
        }
    }

    public delegate void ItemSelectEvent(UIMenu sender, UIMenuItem selectedItem, int index);

    public class UIMenu
    {
        public UIMenu ParentMenu { get; set; }
        public UIMenuItem ParentItem { get; set; }
        public UIMenu NextMenu { get; set; }
        public UIMenuItem BindingMenuItem { get; set; }

        public int SelectedIndex = 0;
        public bool IsVisible;
        public string Title { get; set; }
        public UIMenuItem SelectedItem;
        protected List<UIMenuItem> _itemList = new List<UIMenuItem>();
        List<BindedItem> _bindedList = new List<BindedItem>();
        public Dictionary<UIMenuItem, UIMenu> Binded { get; }

        DateTime InputTimer;
        static int InputWait = 80;

        /*Title Formatting*/
        public Color TitleColor = Color.FromArgb(255, 255, 255, 255);
        public Color TitleUnderlineColor = Color.FromArgb(140, 0, 255, 255);
        public Color TitleBackgroundColor = Color.FromArgb(144, 0, 0, 0);

        /*Title*/
        public float TitleFont;
        public float yPosTitleBG;
        protected float yPosTitleText;
        protected float TitleBGHeight;
        protected float UnderlineHeight;
        protected float yPosUnderline;

        /*UIMenuItem Formatting*/
        public Color DefaultTextColor = Color.FromArgb(255, 255, 255, 255);
        public Color DefaultBoxColor = Color.FromArgb(144, 0, 0, 0);
        public Color HighlightedItemTextColor = Color.FromArgb(255, 0, 255, 255);
        public Color HighlightedBoxColor = Color.FromArgb(255, 0, 0, 0);

        /*Rectangle box for UIMenuItem objects*/
        public float xPosBG;
        protected float yPosItemBG;
        public float MenuBGWidth;
        protected float heightItemBG;
        protected float posMultiplier;

        protected float ItemTextFontSize;
        protected int ItemTextFontType;
        protected float xPosItemText;
        protected float xPosRightEndOfMenu;
        protected float xPosItemValue;
        protected float yTextOffset;

        protected float ScrollBarWidth;
        protected float xPosScrollBar;

        /*Description Formatting*/
        public Color DescriptionTextColor = Color.FromArgb(255, 0, 0, 0);
        public Color DescriptionBoxColor = Color.FromArgb(150, 0, 255, 255);

        /*Scroll or nah?*/
        bool UseScroll = true;
        int YPosBasedOnScroll;
        int YPosDescBasedOnScroll;
        protected int MaxItemsOnScreen = 15;
        protected int minItem = 0;
        protected int maxItem = 14; //must always be 1 less than MaxItemsOnScreen

        private string AUDIO_LIBRARY = "HUD_FRONTEND_DEFAULT_SOUNDSET";

        private string AUDIO_UPDOWN = "NAV_UP_DOWN";
        private string AUDIO_LEFTRIGHT = "NAV_LEFT_RIGHT";
        private string AUDIO_SELECT = "SELECT";
        private string AUDIO_BACK = "BACK";

        public UIMenu(string title)
        {
            Title = title;

            TitleFont = 0.9f; //TitleFont = 1.1f; for no-value fit.
            yPosTitleBG = 0.050f;
            TitleBGHeight = 0.07f; //0.046f
            ItemTextFontSize = 0.452f;
            ItemTextFontType = 4;
            xPosBG = 0.22f; //xPosBG = 0.13f; for no-value fit.
            MenuBGWidth = 0.40f; //MenuBGWidth = 0.24f; for no-value fit.
            heightItemBG = 0.035f;
            UnderlineHeight = 0.002f;
            posMultiplier = 0.035f;
            yTextOffset = 0.015f;
            ScrollBarWidth = 0.0055f;
            CalculateMenuPositioning();
        }

        public virtual void CalculateMenuPositioning()
        {
            yPosTitleText = yPosTitleBG - (TitleFont / 35f);
            yPosUnderline = yPosTitleBG + (TitleBGHeight / 2) + (UnderlineHeight / 2);
            yPosItemBG = yPosUnderline + (UnderlineHeight / 2) + (heightItemBG / 2); //0.0655f;
            xPosItemText = (xPosBG - MenuBGWidth / 2) + 0.0055f;
            xPosRightEndOfMenu = xPosBG + MenuBGWidth / 2; //will Right Justify
            xPosScrollBar = xPosRightEndOfMenu - (ScrollBarWidth / 2);
            xPosItemValue = xPosScrollBar - (ScrollBarWidth / 2);
        }

        public void MaxItemsInMenu(int number)
        {
            MaxItemsOnScreen = number;
            maxItem = number - 1;
        }

        public void AddMenuItem(UIMenuItem item)
        {
            _itemList.Add(item);
        }

        public void BindItemToSubmenu(UIMenu submenu, UIMenuItem itemToBindTo)
        {
            submenu.ParentMenu = this;
            submenu.ParentItem = itemToBindTo;
            /*if (Binded.ContainsKey(itemToBindTo))
                Binded[itemToBindTo] = submenu;
            else
                Binded.Add(itemToBindTo, submenu);*/
            _bindedList.Add(new BindedItem { BindedSubmenu = submenu, BindedItemToSubmenu = itemToBindTo });
        }

        public List<UIMenuItem> UIMenuItemList
        {
            get { return _itemList; }
            set { _itemList = value; }
        }

        public virtual void Draw()
        {
            if (IsVisible)
            {
                DisplayMenu();
                DisableControls();
                DrawScrollBar();
                ManageCurrentIndex();
                /*if (SelectedItem is UIMenuListItem)
                {
                    SelectedItem.ChangeListIndex();
                }*/

                if (/*BindingMenuItem != null && NextMenu != null*/ _bindedList.Count > 0)
                {
                    if (JustPressedAccept() && /*BindingMenuItem == SelectedItem*/ _bindedList.Any(bind => bind.BindedItemToSubmenu == SelectedItem))
                    {
                        IsVisible = false;

                        foreach (var bind in _bindedList.Where(bind => bind.BindedItemToSubmenu == SelectedItem))
                        {
                            bind.BindedSubmenu.IsVisible = true;
                            bind.BindedSubmenu.InputTimer = DateTime.Now.AddMilliseconds(350);
                        }

                        InputTimer = DateTime.Now.AddMilliseconds(350);
                        //return;
                    }
                }

                if (JustPressedCancel())
                {
                    IsVisible = false;

                    if (ParentMenu != null)
                    {
                        ParentMenu.IsVisible = true;
                        ParentMenu.InputTimer = DateTime.Now.AddMilliseconds(350);
                    }

                    InputTimer = DateTime.Now.AddMilliseconds(350);
                    //return;
                }
            }
        }


        protected void DisplayMenu()
        {
            DrawCustomText(Title, TitleFont, 1, TitleColor.R, TitleColor.G, TitleColor.B, TitleColor.A, xPosBG, yPosTitleText, TextJustification.Center); //Draw title text
            DrawRectangle(xPosBG, yPosTitleBG, MenuBGWidth, TitleBGHeight, TitleBackgroundColor.R, TitleBackgroundColor.G, TitleBackgroundColor.B, TitleBackgroundColor.A); //Draw main rectangle
            DrawRectangle(xPosBG, yPosUnderline, MenuBGWidth, UnderlineHeight, TitleUnderlineColor.R, TitleUnderlineColor.G, TitleUnderlineColor.B, TitleUnderlineColor.A); //Draw rectangle as underline of title

            foreach (UIMenuItem item in _itemList)
            {
                bool ScrollOrNotDecision = (UseScroll && _itemList.IndexOf(item) >= minItem && _itemList.IndexOf(item) <= maxItem) || !UseScroll;
                if (ScrollOrNotDecision)
                {
                    YPosBasedOnScroll = UseScroll && _itemList.Count > MaxItemsOnScreen ? CalculatePosition(_itemList.IndexOf(item), minItem, maxItem, 0, MaxItemsOnScreen - 1) : _itemList.IndexOf(item);
                    YPosDescBasedOnScroll = UseScroll && _itemList.Count > MaxItemsOnScreen ? MaxItemsOnScreen : _itemList.Count;

                    if (_itemList.IndexOf(item) == SelectedIndex)
                    {
                        DrawCustomText(item.Text, ItemTextFontSize, ItemTextFontType, HighlightedItemTextColor.R, HighlightedItemTextColor.G, HighlightedItemTextColor.B, HighlightedItemTextColor.A, xPosItemText, yPosItemBG - yTextOffset + YPosBasedOnScroll * posMultiplier); //Draw highlighted item text

                        if (item.Value != null)
                        { DrawCustomText(Convert.ToString(item.Value), ItemTextFontSize, ItemTextFontType, HighlightedItemTextColor.R, HighlightedItemTextColor.G, HighlightedItemTextColor.B, HighlightedItemTextColor.A, xPosItemValue, yPosItemBG - yTextOffset + YPosBasedOnScroll * posMultiplier, TextJustification.Right); } //Draw highlighted item value

                        DrawRectangle(xPosBG, yPosItemBG + YPosBasedOnScroll * posMultiplier, MenuBGWidth, heightItemBG, HighlightedBoxColor.R, HighlightedBoxColor.G, HighlightedBoxColor.B, HighlightedBoxColor.A); //Draw rectangle over highlighted text

                        if (item.Description != null)
                        {
                            foreach (string desc in item.DescriptionTexts)
                            {
                                DrawCustomText(desc, ItemTextFontSize, ItemTextFontType, DescriptionTextColor.R, DescriptionTextColor.G, DescriptionTextColor.B, DescriptionTextColor.A, xPosItemText, yPosItemBG - yTextOffset + (item.DescriptionTexts.IndexOf(desc) + YPosDescBasedOnScroll) * posMultiplier, TextJustification.Left, false); // Draw description text at bottom of menu
                                DrawRectangle(xPosBG, yPosItemBG + (item.DescriptionTexts.IndexOf(desc) + YPosDescBasedOnScroll) * posMultiplier, MenuBGWidth, heightItemBG, DescriptionBoxColor.R, DescriptionBoxColor.G, DescriptionBoxColor.B, DescriptionBoxColor.A); //Draw rectangle over description text at bottom of the list.
                            }
                        }

                        SelectedItem = item;
                    }
                    else
                    {
                        DrawCustomText(item.Text, ItemTextFontSize, ItemTextFontType, DefaultTextColor.R, DefaultTextColor.G, DefaultTextColor.B, DefaultTextColor.A, xPosItemText, yPosItemBG - yTextOffset + YPosBasedOnScroll * posMultiplier); //Draw item text

                        if (item.Value != null)
                        { DrawCustomText(Convert.ToString(item.Value), ItemTextFontSize, ItemTextFontType, DefaultTextColor.R, DefaultTextColor.G, DefaultTextColor.B, DefaultTextColor.A, xPosItemValue, yPosItemBG - yTextOffset + YPosBasedOnScroll * posMultiplier, TextJustification.Right); } //Draw item value

                        DrawRectangle(xPosBG, yPosItemBG + YPosBasedOnScroll * posMultiplier, MenuBGWidth, heightItemBG, DefaultBoxColor.R, DefaultBoxColor.G, DefaultBoxColor.B, DefaultBoxColor.A); //Draw background rectangles around all items.
                    }
                }
            }

            //DevMenuPositioner();
        }

        void DevMenuPositioner()
        {
            if (Game.IsKeyPressed(Keys.NumPad6))
            {
                ItemTextFontSize = (float)Math.Round(ItemTextFontSize + 0.001, 3);
            }
            if (Game.IsKeyPressed(Keys.NumPad4))
            {
                ItemTextFontSize = (float)Math.Round(ItemTextFontSize - 0.001, 3);
            }
            if (Game.IsKeyPressed(Keys.NumPad8))
            {
                heightItemBG = (float)Math.Round(heightItemBG + 0.001, 3);
            }
            if (Game.IsKeyPressed(Keys.NumPad2))
            {
                heightItemBG = (float)Math.Round(heightItemBG - 0.001, 3);
            }
            if (Game.IsKeyPressed(Keys.NumPad9))
            {
                posMultiplier = (float)Math.Round(posMultiplier + 0.001, 3);
            }
            if (Game.IsKeyPressed(Keys.NumPad7))
            {
                posMultiplier = (float)Math.Round(posMultiplier - 0.001, 3);
            }
            if (Game.IsKeyPressed(Keys.NumPad3))
            {
                yTextOffset = (float)Math.Round(yTextOffset + 0.001, 3);
            }
            if (Game.IsKeyPressed(Keys.NumPad1))
            {
                yTextOffset = (float)Math.Round(yTextOffset - 0.001, 3);
            }
            CalculateMenuPositioning();
            UI.ShowSubtitle("ItemTextFontSize: " + ItemTextFontSize + ", heightItemBG: " + heightItemBG + ", posMultiplier: " + posMultiplier + ", yTextOffset: " + yTextOffset);
        }

        protected void DrawScrollBar()
        {
            if (UseScroll && _itemList.Count > MaxItemsOnScreen)
            {
                //Top Y: 0.0632f
                //Bottom Y: 0.2840f

                DrawRectangle(/*xPosBG - 0.00275f + MenuBGWidth / 2*/ xPosScrollBar, CalculateScroll(SelectedIndex, 0, _itemList.Count - 1, yPosItemBG, yPosItemBG + (MaxItemsOnScreen - 1) * posMultiplier), ScrollBarWidth, heightItemBG, TitleUnderlineColor.R, TitleUnderlineColor.G, TitleUnderlineColor.B, TitleUnderlineColor.A);
            }
        }

        int CalculatePosition(int input, int inputMin, int inputMax, int outputMin, int outputMax)
        {
            //http://stackoverflow.com/questions/22083199/method-for-calculating-a-value-relative-to-min-max-values
            //Making sure bounderies arent broken...
            if (input > inputMax)
            {
                input = inputMax;
            }
            if (input < inputMin)
            {
                input = inputMin;
            }
            //Return value in relation to min og max

            double position = (double)(input - inputMin) / (inputMax - inputMin);

            int relativeValue = (int)(position * (outputMax - outputMin)) + outputMin;

            return relativeValue;
        }

        float CalculateScroll(float input, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            //http://stackoverflow.com/questions/22083199/method-for-calculating-a-value-relative-to-min-max-values
            //Making sure bounderies arent broken...
            if (input > inputMax)
            {
                input = inputMax;
            }
            if (input < inputMin)
            {
                input = inputMin;
            }
            //Return value in relation to min og max

            double position = (double)(input - inputMin) / (inputMax - inputMin);

            float relativeValue = (float)(position * (outputMax - outputMin)) + outputMin;

            return relativeValue;
        }

        enum TextJustification
        {
            Center = 0,
            Left,
            Right //requires SET_TEXT_WRAP
        }

        void DrawCustomText(string Message, float FontSize, int FontType, int Red, int Green, int Blue, int Alpha, float XPos, float YPos, TextJustification justifyType = TextJustification.Left, bool ForceTextWrap = false)
        {
            Function.Call(Hash.SET_TEXT_SCALE, 0.0f, FontSize);
            Function.Call(Hash.SET_TEXT_FONT, FontType);
            Function.Call(Hash.SET_TEXT_COLOUR, Red, Green, Blue, Alpha);
            //Function.Call(Hash.SET_TEXT_DROPSHADOW, 0, 0, 0, 0, 0);
            Function.Call(Hash.SET_TEXT_JUSTIFICATION, (int)justifyType);
            if (justifyType == TextJustification.Right || ForceTextWrap)
            {
                Function.Call(Hash.SET_TEXT_WRAP, xPosItemText, xPosItemValue);
            }
            Function.Call(Hash._SET_TEXT_ENTRY, "STRING"); //Required, don't change this! AKA BEGIN_TEXT_COMMAND_DISPLAY_TEXT
            //Function.Call(Hash._0x54CE8AC98E120CAB, "STRING"); //Required, don't change this! AKA BEGIN_TEXT_COMMAND_WIDTH
            Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, Message);
            Function.Call(Hash._DRAW_TEXT, XPos, YPos); //AKA END_TEXT_COMMAND_DISPLAY_TEXT
            //Function.Call<float>(Hash._0x85F061DA64ED2F67, XPos, YPos); //AKA END_TEXT_COMMAND_GET_WIDTH
        }

        void DrawRectangle(float BgXpos, float BgYpos, float BgWidth, float BgHeight, int bgR, int bgG, int bgB, int bgA)
        {
            Function.Call(Hash.DRAW_RECT, BgXpos, BgYpos, BgWidth, BgHeight, bgR, bgG, bgB, bgA);
        }

        protected virtual void ManageCurrentIndex()
        {
            if (JustPressedUp())
            {
                if (SelectedIndex > 0 && SelectedIndex <= _itemList.Count - 1)
                {
                    SelectedIndex--;
                    if (SelectedIndex < minItem && minItem > 0)
                    {
                        minItem--;
                        maxItem--;
                    }
                }
                else if (SelectedIndex == 0)
                {
                    SelectedIndex = _itemList.Count - 1;
                    minItem = _itemList.Count - MaxItemsOnScreen;
                    maxItem = _itemList.Count - 1;
                }
                else
                {
                    SelectedIndex = _itemList.Count - 1;
                    minItem = _itemList.Count - MaxItemsOnScreen;
                    maxItem = _itemList.Count - 1;
                }

                if (IsHoldingSpeedupControl())
                {
                    InputTimer = DateTime.Now.AddMilliseconds(20);
                }
                else
                {
                    InputTimer = DateTime.Now.AddMilliseconds(InputWait);
                }
            }

            if (JustPressedDown())
            {
                if (SelectedIndex >= 0 && SelectedIndex < _itemList.Count - 1)
                {
                    SelectedIndex++;
                    if (SelectedIndex >= maxItem + 1)
                    {
                        minItem++;
                        maxItem++;
                    }
                }
                else if (SelectedIndex == _itemList.Count - 1)
                {
                    SelectedIndex = 0;
                    minItem = 0;
                    maxItem = MaxItemsOnScreen - 1;
                }
                else
                {
                    SelectedIndex = 0;
                    minItem = 0;
                    maxItem = MaxItemsOnScreen - 1;
                }

                if (IsHoldingSpeedupControl())
                {
                    InputTimer = DateTime.Now.AddMilliseconds(20);
                }
                else
                {
                    InputTimer = DateTime.Now.AddMilliseconds(InputWait);
                }
            }
        }

        List<Control> ControlsToDisable = new List<Control>
            {
                Control.FrontendAccept,
                Control.FrontendAxisX,
                Control.FrontendAxisY,
                Control.FrontendDown,
                Control.FrontendUp,
                Control.FrontendLeft,
                Control.FrontendRight,
                Control.FrontendCancel,
                Control.FrontendSelect,
                Control.CharacterWheel,
                Control.CursorScrollDown,
                Control.CursorScrollUp,
                Control.CursorX,
                Control.CursorY,
                /*Control.MoveUpDown,
                Control.MoveLeftRight,
                Control.Sprint,
                Control.Jump,*/
                Control.Enter,
                Control.VehicleExit,
                //Control.VehicleAccelerate,
                //Control.VehicleBrake,
                //Control.VehicleMoveLeftRight,
                Control.VehicleFlyYawLeft,
                Control.FlyLeftRight,
                Control.FlyUpDown,
                Control.VehicleFlyYawRight,
                //Control.VehicleHandbrake,
                Control.VehicleRadioWheel,
                Control.VehicleRoof,
                Control.VehicleHeadlight,
                Control.VehicleCinCam,
                Control.Phone,
            };

        protected void DisableControls()
        {
            foreach (var con in ControlsToDisable)
            {
                Game.DisableControlThisFrame(0, con);
                Game.DisableControlThisFrame(1, con);
                Game.DisableControlThisFrame(2, con);
            }
        }

        public bool JustPressedUp()
        {
            if (Game.IsControlPressed(2, Control.PhoneUp) || Game.IsKeyPressed(Keys.NumPad8) || Game.IsKeyPressed(Keys.Up))
            {
                if (InputTimer < DateTime.Now)
                {
                    Game.PlaySound(AUDIO_UPDOWN, AUDIO_LIBRARY);
                    return true;
                }
            }
            return false;
        }

        public bool JustPressedDown()
        {
            if (Game.IsControlPressed(2, Control.PhoneDown) || Game.IsKeyPressed(Keys.NumPad2) || Game.IsKeyPressed(Keys.Down))
            {
                if (InputTimer < DateTime.Now)
                {
                    Game.PlaySound(AUDIO_UPDOWN, AUDIO_LIBRARY);
                    return true;
                }
            }
            return false;
        }

        public bool JustPressedLeft()
        {
            if (Game.IsControlPressed(2, Control.PhoneLeft) || Game.IsKeyPressed(Keys.NumPad4) || Game.IsKeyPressed(Keys.Left))
            {
                if (InputTimer < DateTime.Now)
                {
                    Game.PlaySound(AUDIO_LEFTRIGHT, AUDIO_LIBRARY);
                    return true;
                }
            }
            return false;
        }

        public bool JustPressedRight()
        {
            if (Game.IsControlPressed(2, Control.PhoneRight) || Game.IsKeyPressed(Keys.NumPad6) || Game.IsKeyPressed(Keys.Right))
            {
                if (InputTimer < DateTime.Now)
                {
                    Game.PlaySound(AUDIO_LEFTRIGHT, AUDIO_LIBRARY);
                    return true;
                }
            }
            return false;
        }

        public bool JustPressedAccept()
        {
            if (Game.IsControlPressed(2, Control.PhoneSelect) || Game.IsKeyPressed(Keys.NumPad5) || Game.IsKeyPressed(Keys.Enter))
            {
                if (InputTimer < DateTime.Now)
                {
                    Game.PlaySound(AUDIO_SELECT, AUDIO_LIBRARY);
                    //InputTimer = Game.GameTime + 350;
                    return true;
                }
            }
            return false;
        }

        public bool JustPressedCancel()
        {
            if (Game.IsControlPressed(2, Control.PhoneCancel) || Game.IsKeyPressed(Keys.NumPad0) || Game.IsKeyPressed(Keys.Back))
            {
                if (InputTimer < DateTime.Now)
                {
                    Game.PlaySound(AUDIO_BACK, AUDIO_LIBRARY);
                    //InputTimer = Game.GameTime + InputWait;
                    return true;
                }
            }
            return false;
        }

        bool IsHoldingSpeedupControl()
        {
            if (Game.CurrentInputMode == InputMode.GamePad)
            {
                return Game.IsControlPressed(2, Control.VehicleHandbrake);
            }
            else
            {
                return Game.IsControlPressed(2, Control.Sprint);
            }
        }

        public void SetInputWait(int ms = 350)
        {
            InputTimer = DateTime.Now.AddMilliseconds(ms);
        }

        public bool ControlBoolValue(UIMenuItem item, bool boolToControl)
        {
            item.Value = boolToControl;

            if (SelectedItem == item)
            {
                if (JustPressedAccept())
                {
                    boolToControl = !boolToControl;
                    item.Value = boolToControl;
                    InputTimer = DateTime.Now.AddMilliseconds(InputWait);
                    return boolToControl;
                }
            }
            return boolToControl;
        }

        public float ControlFloatValue(UIMenuItem item, float numberToControl, float incrementValue, float incrementValueFast, int decimals = 2)
        {
            item.Value = "< " + numberToControl + " >";

            if (SelectedItem == item)
            {
                if (JustPressedLeft())
                {
                    if (IsHoldingSpeedupControl())
                    {
                        numberToControl -= incrementValueFast;
                    }
                    else
                    {
                        numberToControl -= incrementValue;
                    }

                    item.Value = "< " + numberToControl + " >";

                    InputTimer = DateTime.Now.AddMilliseconds(InputWait);

                    return (float)Math.Round(numberToControl, decimals);
                }
                if (JustPressedRight())
                {
                    if (IsHoldingSpeedupControl())
                    {
                        numberToControl += incrementValueFast;
                    }
                    else
                    {
                        numberToControl += incrementValue;
                    }

                    item.Value = "< " + numberToControl + " >";

                    InputTimer = DateTime.Now.AddMilliseconds(InputWait);

                    return (float)Math.Round(numberToControl, decimals);
                }
            }
            return numberToControl;
        }

        public int ControlIntValue(UIMenuItem item, int numberToControl, int incrementValue, int incrementValueFast)
        {
            item.Value = "< " + numberToControl + " >";

            if (SelectedItem == item)
            {
                if (JustPressedLeft())
                {
                    if (IsHoldingSpeedupControl())
                    {
                        numberToControl -= incrementValueFast;
                    }
                    else
                    {
                        numberToControl -= incrementValue;
                    }

                    item.Value = "< " + numberToControl + " >";

                    InputTimer = DateTime.Now.AddMilliseconds(InputWait);

                    return numberToControl;
                }
                if (JustPressedRight())
                {
                    if (IsHoldingSpeedupControl())
                    {
                        numberToControl += incrementValueFast;
                    }
                    else
                    {
                        numberToControl += incrementValue;
                    }

                    item.Value = "< " + numberToControl + " >";

                    InputTimer = DateTime.Now.AddMilliseconds(InputWait);

                    return numberToControl;
                }
            }
            return numberToControl;
        }
    }

    public class UIMenuDisplayOnly : UIMenu
    {
        public UIMenuDisplayOnly(string text) : base(text)
        {
            base.TitleFont = 0.5f;
            base.yPosTitleBG = 0.50f;
            base.yPosTitleText = yPosTitleBG - (TitleFont / 35f);
            base.TitleBGHeight = 0.04f; //0.046f
            base.xPosBG = 0.125f;
            base.MenuBGWidth = 0.20f;
            base.heightItemBG = 0.025f;
            base.UnderlineHeight = 0.002f;
            base.yPosUnderline = yPosTitleBG + (TitleBGHeight / 2) + (UnderlineHeight / 2);
            base.yPosItemBG = yPosUnderline + (UnderlineHeight / 2) + (heightItemBG / 2); //0.0655f;
            base.posMultiplier = 0.025f;
            base.yTextOffset = 0.015f;
            base.xPosItemText = xPosBG - MenuBGWidth / 2;
            base.xPosRightEndOfMenu = xPosBG + MenuBGWidth / 2; //will Right Justify
            base.ScrollBarWidth = 0.0055f;
            base.xPosScrollBar = xPosRightEndOfMenu - (ScrollBarWidth / 2);
            base.xPosItemValue = xPosScrollBar - (ScrollBarWidth / 2);
            CalculateMenuPositioning();

            MaxItemsInMenu(8);
        }

        public override void CalculateMenuPositioning()
        {
            yPosTitleText = yPosTitleBG - (TitleFont / 35f);
            yPosUnderline = yPosTitleBG + (TitleBGHeight / 2) + (UnderlineHeight / 2);
            yPosItemBG = yPosUnderline + (UnderlineHeight / 2) + (heightItemBG / 2); //0.0655f;
            xPosItemText = xPosBG - MenuBGWidth / 2;
            xPosRightEndOfMenu = xPosBG + MenuBGWidth / 2; //will Right Justify
            xPosScrollBar = xPosRightEndOfMenu - (ScrollBarWidth / 2);
            xPosItemValue = xPosScrollBar - (ScrollBarWidth / 2);
        }

        public override void Draw()
        {
            if (IsVisible)
            {
                DisplayMenu();
                DisableControls();
                DrawScrollBar();
                //ManageCurrentIndex();
            }
        }

        protected override void ManageCurrentIndex()
        {
            //base.ManageCurrentIndex();
        }

        public void GoToNextItem()
        {
            SelectedIndex++;
            if (SelectedIndex >= maxItem + 1)
            {
                minItem++;
                maxItem++;
            }
        }

        public void GoToFirstItem()
        {
            SelectedIndex = 0;
            minItem = 0;
            maxItem = MaxItemsOnScreen - 1;
        }

        public void GoToPreviousItem()
        {
            SelectedIndex--;
            if (SelectedIndex < minItem && minItem > 0)
            {
                minItem--;
                maxItem--;
            }
        }

        public void GoToLastItem()
        {
            SelectedIndex = _itemList.Count - 1;
            minItem = _itemList.Count - MaxItemsOnScreen;
            maxItem = _itemList.Count - 1;
        }
    }

    public class UIMenuItem
    {
        string _text { get; set; }
        dynamic _value { get; set; }
        string _description { get; set; }
        public List<string> DescriptionTexts;

        public UIMenuItem(string text)
        {
            _text = text;
        }

        public UIMenuItem(string text, dynamic value)
        {
            _text = text;
            _value = value;
        }

        public UIMenuItem(string text, dynamic value, string description)
        {
            _text = text;
            _value = value;
            _description = description;
            DescriptionTexts = description.SplitOn(90);
        }

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public dynamic Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public string Description
        {
            get { return _description; }
            set { DescriptionTexts = value.SplitOn(90); _description = value; }
        }

        public virtual void ChangeListIndex() { }
    }

    /*public class UIMenuListItem : UIMenuItem
    {
        public List<dynamic> List { get; set; }
        public int SelectedIndex = 0;

        public UIMenuListItem(string text, dynamic value, string description, List<dynamic> list)
        {
            this.Text = text;
            this.Value = value;
            this.Description = description;
            List = list;
        }

        public override void ChangeListIndex()
        {

        }
    }*/

    class BindedItem
    {
        private UIMenu _menu;
        private UIMenuItem _item;

        public UIMenu BindedSubmenu
        {
            get { return _menu; }
            set { _menu = value; }
        }

        public UIMenuItem BindedItemToSubmenu
        {
            get { return _item; }
            set { _item = value; }
        }
    }

    /*public static class SplitStringByLength
    {
        public static IEnumerable<string> SplitByLength(this string str, int maxLength)
        {
            for (int index = 0; index < str.Length; index += maxLength)
            {
                yield return str.Substring(index, Math.Min(maxLength, str.Length - index));
            }
        }
    }*/

    // using System.Text.RegularExpressions;
    public static class StringExtensions
    {

        /// <summary>Use this function like string.Split but instead of a character to split on, 
        /// use a maximum line width size. This is similar to a Word Wrap where no words will be split.</summary>
        /// Note if the a word is longer than the maxcharactes it will be trimmed from the start.
        /// <param name="initial">The string to parse.</param>
        /// <param name="MaxCharacters">The maximum size.</param>
        /// <remarks>This function will remove some white space at the end of a line, but allow for a blank line.</remarks>
        /// 
        /// <returns>An array of strings.</returns>
        public static List<string> SplitOn(this string initial, int MaxCharacters)
        {

            List<string> lines = new List<string>();

            if (string.IsNullOrEmpty(initial) == false)
            {
                string targetGroup = "Line";
                string pattern = string.Format(@"(?<{0}>.{{1,{1}}})(?:\W|$)", targetGroup, MaxCharacters);

                lines = Regex.Matches(initial, pattern, RegexOptions.Multiline | RegexOptions.CultureInvariant)
                             .OfType<Match>()
                             .Select(mt => mt.Groups[targetGroup].Value)
                             .ToList();
            }
            return lines;
        }
    }
}
