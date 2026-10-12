using UnityEngine;
using UnityEngine.UI;

namespace Shiftbound
{
    // Camera-space phone HUD shares the contact router's safe-area coordinates.
    // No raycaster: production touch routing remains the sole input owner.
    public sealed class PhoneHUDCanvas : MonoBehaviour
    {
        PhoneControls phone;
        Canvas canvas;
        CanvasScaler scaler;
        RectTransform safe;
        Font font;
        Sprite circle;
        Sprite panel;
        Image stick, jump, shift, thumb, pause, hintBox, noticeBox;
        Text worldText, timeText, jumpText, hint, notice;
        GameObject controls;
        RectTransform menu;
        Text menuTitle;
        readonly Text[] menuLabels=new Text[11];
        readonly Image[] menuRows=new Image[11],menuTracks=new Image[11],menuFills=new Image[11];
        public bool Ready => menu!=null;
        int shownTenths=-1;
        void Start()
        {
            phone=GetComponent<PhoneControls>();
            if(!phone.Visible){enabled=false; return;}
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var root=new GameObject("Phone HUD",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));
            root.transform.SetParent(transform,false);
            canvas=root.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceCamera;
            canvas.worldCamera=Camera.main; canvas.planeDistance=.5f; canvas.sortingOrder=100;
            scaler=root.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;
            safe=Area(root.transform,"Safe area",new Rect(0,0,phone.Width,720));
            var texture=new Texture2D(64,64,TextureFormat.RGBA32,false);
            for(int y=0;y<64;y++)for(int x=0;x<64;x++)
                texture.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(32-Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f)))));
            texture.Apply(); circle=Sprite.Create(texture,new Rect(0,0,64,64),new Vector2(.5f,.5f));
            var rounded=new Texture2D(48,48,TextureFormat.RGBA32,false);
            for(int y=0;y<48;y++)for(int x=0;x<48;x++)
            {
                Vector2 nearest=new Vector2(Mathf.Clamp(x,11,36),Mathf.Clamp(y,11,36));
                rounded.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(12f-Vector2.Distance(new Vector2(x,y),nearest))));
            }
            rounded.Apply();panel=Sprite.Create(rounded,new Rect(0,0,48,48),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(12,12,12,12));
            Image box=Box(safe,"World",new Rect(20,20,250,68));
            Label(box.transform,"SHIFTBOUND / WORLD",new Rect(18,7,220,20),13,TextAnchor.MiddleLeft);
            worldText=Label(box.transform,"PRESENT",new Rect(18,27,220,31),22,TextAnchor.MiddleLeft);
            box=Box(safe,"Timer",new Rect(phone.Width-167,20,147,58));
            timeText=Label(box.transform,"00:00.0",new Rect(8,8,131,42),23,TextAnchor.MiddleCenter);
            controls=Area(safe,"Controls",new Rect(0,0,phone.Width,720)).gameObject;
            stick=Disc(controls.transform,"Move",phone.Router.Stick); Label(stick.transform,"MOVE",new Rect(0,0,192,192),22,TextAnchor.MiddleCenter);
            jump=Disc(controls.transform,"Jump",phone.Router.Jump); jumpText=Label(jump.transform,"JUMP",new Rect(0,0,124,124),22,TextAnchor.MiddleCenter);
            shift=Disc(controls.transform,"Shift",phone.Router.Shift); Label(shift.transform,"SHIFT",new Rect(0,0,100,100),22,TextAnchor.MiddleCenter);
            thumb=Disc(controls.transform,"Thumb",new Rect(0,0,44,44)); thumb.color=new Color(.65f,.9f,.87f,.9f);
            pause=Box(controls.transform,"Pause",phone.PauseRect); Label(pause.transform,"II",new Rect(0,0,64,60),22,TextAnchor.MiddleCenter);
            hintBox=Box(safe,"Hint",new Rect(phone.Width*.5f-270,102,540,52));
            hint=Label(hintBox.transform,"",new Rect(16,4,508,44),17,TextAnchor.MiddleCenter);
            noticeBox=Box(safe,"Notice",new Rect(phone.Width*.5f-170,164,340,38));
            notice=Label(noticeBox.transform,"",new Rect(10,3,320,32),16,TextAnchor.MiddleCenter);
            menu=Area(safe,"Menu",new Rect(0,0,phone.Width,720));
            Box(menu,"Backdrop",new Rect(0,0,phone.Width,720)).color=new Color(.025f,.05f,.07f,.84f);
            Box(menu,"Settings card",new Rect(phone.Width*.5f-272,68,544,615));
            menuTitle=Label(menu,"PAUSED",new Rect(phone.Width*.5f-250,80,500,42),25,TextAnchor.MiddleLeft);
            for(int row=0;row<11;row++)
            {
                menuRows[row]=Box(menu,"Setting "+row,phone.MenuRect(row));
                menuRows[row].color=new Color(.11f,.19f,.22f,.88f);
                menuLabels[row]=Label(menuRows[row].transform,"",new Rect(14,0,472,46),18,TextAnchor.MiddleLeft);
                if(row>=2 && row<=6)
                {
                    menuLabels[row].rectTransform.sizeDelta=new Vector2(190,46);
                    menuTracks[row]=Box(menuRows[row].transform,"Track",new Rect(210,19,275,8));
                    menuTracks[row].color=new Color(.23f,.31f,.33f);
                    menuFills[row]=Box(menuRows[row].transform,"Value",new Rect(210,19,275,8));
                    menuFills[row].color=new Color(.53f,.81f,.77f);
                }
            }
        }
        void LateUpdate()
        {
            if(canvas==null || GameFlow.Instance==null)return;
            var flow=GameFlow.Instance; phone.Layout(); scaler.scaleFactor=phone.Scale;
            Place(safe,new Rect(Screen.safeArea.x/phone.Scale,(Screen.height-Screen.safeArea.yMax)/phone.Scale,phone.Width,720));
            worldText.text=flow.worlds.IsAltered?"OVERGROWN":"PRESENT";
            int tenths=Mathf.FloorToInt(flow.Elapsed*10f);
            if(tenths!=shownTenths){shownTenths=tenths;timeText.text=PremiumHUD.FormatTime(flow.Elapsed);}
            Color accent=flow.worlds.IsAltered?new Color(.72f,.86f,.44f):new Color(.54f,.83f,.89f);
            worldText.color=accent;
            jump.color=phone.Router.JumpHeld?new Color(.21f,.49f,.51f,.94f):new Color(.035f,.085f,.12f,.70f);
            shift.color=flow.worlds.IsReady?new Color(.11f,.26f,.29f,.82f):new Color(.075f,.13f,.15f,.70f);
            Place((RectTransform)timeText.transform.parent,new Rect(phone.Width-167,20,147,58));
            controls.SetActive(flow.IsPlaying);
            UpdateMenu(flow);
            Place(stick.rectTransform,phone.Router.Stick); Place(jump.rectTransform,phone.Router.Jump); Place(shift.rectTransform,phone.Router.Shift);
            StretchLabel(stick); StretchLabel(jump); StretchLabel(shift); Place(pause.rectTransform,phone.PauseRect);
            Vector2 point=phone.Router.Stick.center+new Vector2(phone.Router.Move.x,-phone.Router.Move.y)*phone.Router.Stick.width*.25f;
            Place(thumb.rectTransform,new Rect(point.x-22,point.y-22,44,44)); jumpText.text=phone.Router.JumpHeld?"HOLD":"JUMP";
            string guidance="";
            if(flow.HasShiftBridgeGuidance && flow.CheckpointHintAge<6f)
                guidance=flow.worlds.IsAltered?"Bridge solid. Hold JUMP to cross.":
                    "Blue roofs are previews. SHIFT makes them solid.";
            else if(flow.IsPlaying && flow.CheckpointHintAge<5f && !string.IsNullOrEmpty(flow.CheckpointHint))
                guidance=flow.CheckpointHint.Replace("SPACE","JUMP").Replace("Space","JUMP").Replace("press Shift","tap SHIFT");
            else if(flow.IsPlaying && flow.Elapsed<7f)
                guidance="Hold JUMP for distance. Slide to SHIFT midair.\nDrag the free right area to look.";
            hintBox.gameObject.SetActive(guidance.Length>0); hint.text=guidance;
            Place(hintBox.rectTransform,new Rect(phone.Width*.5f-270,102,540,52));
            noticeBox.gameObject.SetActive(!string.IsNullOrEmpty(flow.ActiveNotice)); notice.text=flow.ActiveNotice;
            Place(noticeBox.rectTransform,new Rect(phone.Width*.5f-170,164,340,38));
        }

        void UpdateMenu(GameFlow flow)
        {
            menu.gameObject.SetActive(!flow.IsPlaying);
            if(flow.IsPlaying)return;
            Place(menu,new Rect(0,0,phone.Width,720));
            Place((RectTransform)menu.GetChild(0),new Rect(0,0,phone.Width,720));
            Place((RectTransform)menu.GetChild(1),new Rect(phone.Width*.5f-272,68,544,phone.ShowDiagnostics?615:565));
            Place(menuTitle.rectTransform,new Rect(phone.Width*.5f-250,80,500,42));
            menuTitle.text=flow.IsComplete?"ROOFTOP CLEARED":"PAUSED";
            menuTitle.color=flow.IsComplete?new Color(.95f,.75f,.37f):Color.white;
            for(int row=0;row<11;row++)
            {
                Place(menuRows[row].rectTransform,phone.MenuRect(row));
                menuRows[row].gameObject.SetActive(row<10 || phone.ShowDiagnostics);
            }
            menuLabels[0].text=flow.IsComplete?"Run time   "+PremiumHUD.FormatTime(flow.Elapsed):"RESUME RUN";
            menuLabels[1].text="RESTART FROM BEGINNING";
            menuRows[1].color=flow.IsComplete?new Color(.48f,.34f,.13f,.95f):new Color(.11f,.19f,.22f,.88f);
            menuLabels[2].text="Control size";SetFill(2,Mathf.InverseLerp(.8f,1.35f,PlayerPreferences.ControlScale));
            menuLabels[3].text="Horizontal inset";SetFill(3,PlayerPreferences.ControlInset/90f);
            menuLabels[4].text="Control height";SetFill(4,PlayerPreferences.ControlHeight/100f);
            menuLabels[5].text="Look sensitivity";SetFill(5,Mathf.InverseLerp(.3f,2.5f,PlayerPreferences.TouchSensitivity));
            menuLabels[6].text="Sound volume";SetFill(6,PlayerPreferences.Volume);
            menuLabels[7].text="Camera follow     "+(PlayerPreferences.CameraAssist?"ON":"OFF");
            menuLabels[8].text="Invert vertical     "+(PlayerPreferences.InvertY?"ON":"OFF");
            menuLabels[9].text=PlayerPreferences.LowPower?"Frame cap     30 FPS":"Frame cap     60 FPS";
            menuLabels[10].text="QA: start / stop frame recording";
        }
        void SetFill(int row,float value)=>Place(menuFills[row].rectTransform,new Rect(210,19,Mathf.Max(8,275*value),8));
        static RectTransform Area(Transform parent,string name,Rect area)
        {
            var item=new GameObject(name,typeof(RectTransform)); item.transform.SetParent(parent,false);
            var rect=item.GetComponent<RectTransform>(); Place(rect,area); return rect;
        }
        static void Place(RectTransform rect,Rect area)
        {
            rect.anchorMin=rect.anchorMax=new Vector2(0,1); rect.pivot=new Vector2(0,1);
            rect.anchoredPosition=new Vector2(area.x,-area.y); rect.sizeDelta=area.size;
        }
        Image Box(Transform parent,string name,Rect area)
        {
            Image image=Area(parent,name,area).gameObject.AddComponent<Image>();
            image.sprite=panel;image.type=Image.Type.Sliced;
            image.color=new Color(.035f,.085f,.12f,.87f); image.raycastTarget=false; return image;
        }
        Image Disc(Transform parent,string name,Rect area)
        {
            Image image=Box(parent,name,area); image.sprite=circle;image.type=Image.Type.Simple; image.color=new Color(.04f,.09f,.12f,.62f); return image;
        }
        Text Label(Transform parent,string text,Rect area,int size,TextAnchor align)
        {
            Text label=Area(parent,"Label",area).gameObject.AddComponent<Text>(); label.font=font;
            label.text=text; label.fontSize=size; label.fontStyle=FontStyle.Bold; label.color=Color.white;
            label.alignment=align; label.horizontalOverflow=HorizontalWrapMode.Wrap; label.raycastTarget=false; return label;
        }
        static void StretchLabel(Image image)
        {
            if(image.transform.childCount>0)Place((RectTransform)image.transform.GetChild(0),new Rect(Vector2.zero,image.rectTransform.sizeDelta));
        }
        void OnDestroy(){if(circle!=null){Destroy(circle.texture); Destroy(circle);}}
    }
}
