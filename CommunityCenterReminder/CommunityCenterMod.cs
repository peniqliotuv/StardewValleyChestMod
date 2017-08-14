using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using UIUtilities;
using Object = StardewValley.Object;
using TextBox = UIUtilities.TextBox;

namespace CommunityCenterReminder
{
    public class CommunityCenterMod : Mod
    {
        private static Vector2 ChestVector;
        private static String HoverChestValue;
        private static IClickableMenu LastMenu;
        private static ClickableTextureComponent LabelButton;
        private static bool hovered = false;
        private static TextBox ChestValueBox;
        private static bool leftClickWasDown = false;
        private static InputButton[] oldMenuButton;
        
        public override void Entry(IModHelper helper)
        {
            //Adding event handlers
           // ControlEvents.KeyPressed += this.OnKeyPress;
            TimeEvents.AfterDayStarted += handler;
            GameEvents.UpdateTick += EventUpdateTick;     
            GraphicsEvents.OnPostRenderGuiEvent += PostEventGUIDrawTick;
            GraphicsEvents.OnPostRenderHudEvent += PostEventHUDDrawTick;
        }

        private void handler(object sender, EventArgs e)
        {
            if (Context.IsWorldReady)
            {
                this.GetDonatableItems();
            }
        }

        private List<Object> GetDonatableItems()
        {
            CommunityCenter communityCenter = Game1.getLocationFromName("CommunityCenter") as CommunityCenter;
            SerializableDictionary<int, bool[]> BundleDictionary = communityCenter.bundles;
            
            Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\Bundles");
            foreach (KeyValuePair<string, string> kvp in dictionary)
            {
                string[] Delimit = kvp.Key.Split('/');
                string BundleName = Delimit[0];
                int BundleIndex = Convert.ToInt32(Delimit[1]);
                if (!(BundleIndex >= 23 && BundleIndex <= 26))
                {
                    
                }
            }
            HashSet<Item> PlayerItems = GetAllItemsFromChest();
            for (int i = 0; i < 7; i++)
            {
                Monitor.Log(string.Join("", BundleDictionary[i]));
            }
            return null;
        }

        private List<Chest> GetChests()
        {
            var Chests = new List<Chest>();
            Monitor.Log("Getting all of the chests");

            foreach (GameLocation location in Game1.locations)
            {
                foreach (KeyValuePair<Vector2, Object> pair in location.Objects)
                {
                    Vector2 tile = pair.Key;
                    Chest chest = pair.Value as Chest;
                    if (chest != null && chest.playerChest)
                    {
                        Chests.Add(chest);
                    }
                }
            }
            return Chests;
        }

        private HashSet<Item> GetAllItemsFromChest()
        {
            var Items = new HashSet<Item>();
            List<Chest> Chests = GetChests();
            foreach (Chest chest in Chests)
            {
                Items.UnionWith(chest.items);
            }

            foreach (Item item in Items)
            {
                Monitor.Log(item.DisplayName);
            }
            return Items;
        }

        private string GetChestValue(Chest chest)
        {
            int TotalValue = 0;
            foreach (Item item in chest.items)
            {
                if (item != null)
                {
                    //Monitor.Log($"Item's value: ${(item as Object).sellToStorePrice()}");
                    //TotalValue += (item as Object).sellToStorePrice();
                    Monitor.Log(item.salePrice().ToString());
                    TotalValue += item.salePrice();
                }
            }
            Monitor.Log($"Total Value: {TotalValue}");
            return $"Chest Value: {TotalValue}";
        }
        
        
        void PostEventGUIDrawTick(object sender, EventArgs e)
        {            
            if (LabelButton != null)
            {                                
                Monitor.Log($"LabelButton is not null. Hovered: {hovered}");
                Utility.drawWithShadow(Game1.spriteBatch, LabelButton.texture, new Vector2((float)LabelButton.bounds.X + (float)(LabelButton.sourceRect.Width / 2) * LabelButton.baseScale, (float)LabelButton.bounds.Y + (float)(LabelButton.sourceRect.Height / 2) * LabelButton.baseScale), LabelButton.sourceRect, Color.White, 0f, new Vector2((float)(LabelButton.sourceRect.Width / 2), (float)(LabelButton.sourceRect.Height / 2)), LabelButton.scale, false, 0, -1, -1, 0.35f);
                ChestValueBox.Draw(Game1.spriteBatch, false);
                if (hovered)
                {
                    SpeederIClickableMenu.drawSimpleTooltip(Game1.spriteBatch, LabelButton.hoverText, Game1.smallFont);
                }
                Game1.spriteBatch.Draw(Game1.mouseCursors, new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY()), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 0, 16, 16)), Color.White, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 0);                                
            }            
        }

        
        void PostEventHUDDrawTick(object sender, EventArgs e)
        {            
            if (HoverChestValue != null && HoverChestValue != "" && Game1.activeClickableMenu == null)
            {                
                SpeederIClickableMenu.drawSimpleTooltip(Game1.spriteBatch, HoverChestValue, Game1.smallFont);                
            }
        }
        
        void EventUpdateTick(object sender, EventArgs e)
        {            
            if (Game1.currentLocation == null) return;
            HoverChestValue = null;
            if(LastMenu == null || Game1.activeClickableMenu == null || LastMenu != Game1.activeClickableMenu)
            {
                if(ChestValueBox != null && Game1.keyboardDispatcher.Subscriber == ChestValueBox) Game1.keyboardDispatcher.Subscriber = null;
                LabelButton = null;
                leftClickWasDown = false;
                ChestValueBox = null;                
                if(oldMenuButton != null)
                {
                    Game1.options.menuButton = oldMenuButton;
                    oldMenuButton = null;
                }
            }
            GameLocation currentLocation = Game1.currentLocation;

            Chest openChest = null;            

            foreach(KeyValuePair<Vector2, Object> keyPair in currentLocation.objects)
            {
                if (keyPair.Value is Chest)
                {
                    openChest = (Chest) keyPair.Value;
                    if(openChest.currentLidFrame == 135 && Game1.activeClickableMenu is ItemGrabMenu)
                    {
                        LastMenu = Game1.activeClickableMenu;
                        ChestVector = keyPair.Key;
                        break;
                    }                    
                    if(openChest.getBoundingBox(keyPair.Key).Contains(Game1.getMouseX()+Game1.viewport.X, Game1.getMouseY()+Game1.viewport.Y))
                    {
                        // HoverChestValue = openChest.name;
                        if (openChest != null)
                        {
                            HoverChestValue = GetChestValue(openChest);
                        }
                    }
                    openChest = null;
                }
            }

            if (openChest == null) return;

            if(LabelButton == null)
            {                
                ChestValueBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor);
                ChestValueBox.Text = openChest.name;
                ChestValueBox.X = LastMenu.xPositionOnScreen;
                ChestValueBox.Y = LastMenu.yPositionOnScreen + LastMenu.height - Game1.tileSize*2;
                ChestValueBox.Width = Game1.tileSize * 5;
                LabelButton = new ClickableTextureComponent("label-chest", new Rectangle(ChestValueBox.X + ChestValueBox.Width + Game1.tileSize/2, ChestValueBox.Y, Game1.tileSize, Game1.tileSize), "", "Label Chest", Game1.mouseCursors, new Rectangle(128, 256, 64, 64), (float)Game1.pixelZoom/6f);
            }

            int mouseX = Game1.getOldMouseX();
            int mouseY = Game1.getOldMouseY();
            bool mouseJustReleased = false;
            
            if(leftClickWasDown == true && Mouse.GetState().LeftButton == ButtonState.Released)
            {
                mouseJustReleased = true;                
            }

            LabelButton.tryHover(mouseX, mouseY);
            
            Rectangle ChestValueBoxBoundingBox = new Rectangle(ChestValueBox.X, ChestValueBox.Y, ChestValueBox.Width, ChestValueBox.Height);

            if (ChestValueBoxBoundingBox.Contains(mouseX, mouseY))
            {
                leftClickWasDown = Mouse.GetState().LeftButton == ButtonState.Pressed;

                ChestValueBox.Highlighted = true;        

                if (mouseJustReleased)
                {
                    ChestValueBox.SelectMe();
                    if (oldMenuButton == null)
                    {
                        oldMenuButton = Game1.options.menuButton;
                        Game1.options.menuButton = new InputButton[] { };
                    }
                    //Game1.freezeControls = true;
                }                
            }
            else if(ChestValueBox.Selected == false)
            {
                ChestValueBox.Highlighted = false;
            }

            if (LabelButton.containsPoint(mouseX, mouseY))
            {                
                if (Mouse.GetState().LeftButton == ButtonState.Pressed) leftClickWasDown = true;
                else leftClickWasDown = false;
                hovered = true;

                if (mouseJustReleased && ChestValueBox.Selected)
                {
                    ChestValueBox.Selected = false;
                    ChestValueBox.Highlighted = false;
                    Game1.keyboardDispatcher.Subscriber = null;
                    currentLocation.objects[ChestVector].name = ChestValueBox.Text;
                    Game1.playSound("smallSelect");
                    Game1.options.menuButton = oldMenuButton;
                    oldMenuButton = null;                    
                }
            }
            else
            {
                hovered = false;
            }
        }


    }
}