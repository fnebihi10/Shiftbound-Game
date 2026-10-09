using UnityEngine;
using UnityEngine.UI;

namespace Shiftbound
{
    // Shipped interface and evidence share this Canvas; PhoneControls retains
    // exclusive contact ownership, including the pause/settings sheet.
    public sealed class ProductionHUDCanvas : MonoBehaviour
    {
        PhoneControls phone; Canvas canvas; CanvasScaler scaler; RectTransform safe; Font font;
        Sprite disc, ring, rounded;
        Image stick,jump,shift,thumb,pause,hintBox,noticeBox,menuBox,chord;
        Text worldText,timeText,jumpText,shiftText,hint,notice,menuTitle;
        GameObject controls,menu;
        readonly Text[] rows=new Text[11];
        readonly Image[] tracks=new Image[5],fills=new Image[5];
        float feedbackUntil,lessonUntil; bool learnedAir,bridgeSeen;
        WorldSwitcher.ShiftResult result;
        readonly Color amber=new Color(1,.73f,.29f),cyan=new Color(.48f,.89f,.85f);
        void Start()
        {
            phone=GetComponent<PhoneControls>();font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            disc=Shape(0);ring=Shape(1);rounded=Shape(2);
            var root=new GameObject("Shiftbound interface",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));
            root.transform.SetParent(transform,false);canvas=root.GetComponent<Canvas>();
            canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=Camera.main;canvas.planeDistance=.4f;canvas.sortingOrder=100;
            scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;
            safe=Area(root.transform,"Safe area",new Rect(0,0,phone.Width,720));
            var box=Box(safe,"World",new Rect(20,20,210,61));
            var accent=Box(box.transform,"Accent",new Rect(12,14,4,33));accent.color=cyan;
            Label(box.transform,"SHIFTBOUND",new Rect(28,8,166,16),11,TextAnchor.MiddleLeft).color=new Color(.67f,.77f,.8f);
            worldText=Label(box.transform,"PRESENT",new Rect(28,25,166,26),20,TextAnchor.MiddleLeft);
            box=Box(safe,"Time",new Rect(phone.Width-133,20,113,49));timeText=Label(box.transform,"00:00.0",new Rect(6,5,101,39),21,TextAnchor.MiddleCenter);
            controls=Area(safe,"Two thumb controls",new Rect(0,0,phone.Width,720)).gameObject;
            stick=Disc(controls.transform,"Move",phone.Router.Stick,true);Label(stick.transform,"MOVE",new Rect(0,0,192,192),13,TextAnchor.MiddleCenter).color=new Color(1,1,1,.48f);
            jump=Disc(controls.transform,"Jump",phone.Router.Jump,true);jumpText=Label(jump.transform,"JUMP",new Rect(0,0,124,124),17,TextAnchor.MiddleCenter);
            shift=Disc(controls.transform,"Shift",phone.Router.Shift,true);shiftText=Label(shift.transform,"SHIFT",new Rect(0,0,92,92),15,TextAnchor.MiddleCenter);
            thumb=Disc(controls.transform,"Stick contact",new Rect(0,0,40,40),false);thumb.color=new Color(.75f,.92f,.90f,.68f);
            chord=Box(controls.transform,"Slide affordance",new Rect(0,0,100,3));chord.color=new Color(.48f,.89f,.85f,.22f);
            pause=Box(controls.transform,"Pause",phone.PauseRect);Label(pause.transform,"II",new Rect(0,0,64,60),21,TextAnchor.MiddleCenter);
            hintBox=Box(safe,"Contextual lesson",new Rect(phone.Width*.5f-220,607,440,42));hint=Label(hintBox.transform,"",new Rect(12,4,416,34),16,TextAnchor.MiddleCenter);
            noticeBox=Box(safe,"Action feedback",new Rect(phone.Width*.5f-195,553,390,40));notice=Label(noticeBox.transform,"",new Rect(10,4,370,32),16,TextAnchor.MiddleCenter);
            menu=Area(safe,"Pause and completion",new Rect(0,0,phone.Width,720)).gameObject;
            menuBox=Box(menu.transform,"Sheet",new Rect(phone.Width*.5f-270,68,540,610));menuBox.color=new Color(.025f,.047f,.057f,.96f);
            menuTitle=Label(menu.transform,"PAUSED",new Rect(phone.Width*.5f-250,80,500,42),26,TextAnchor.MiddleCenter);
            for(int row=0;row<11;row++)
            {
                var cell=Box(menu.transform,"Menu row "+row,phone.MenuRect(row));
                cell.color=row==0?new Color(.13f,.3f,.30f,.95f):new Color(.06f,.095f,.109f,.96f);
                rows[row]=Label(cell.transform,"",new Rect(12,0,row>=2&&row<=6?185:476,46),row<2?18:16,TextAnchor.MiddleLeft);
                if(row>=2&&row<=6)
                {
                    tracks[row-2]=Box(cell.transform,"Slider",new Rect(210,19,275,8));tracks[row-2].color=new Color(.19f,.25f,.28f);
                    fills[row-2]=Box(cell.transform,"Value",new Rect(210,19,140,8));fills[row-2].color=cyan;
                }
            }
            if(GameFlow.Instance!=null)GameFlow.Instance.worlds.ShiftAttempted+=OnShift;
        }
        void OnShift(WorldSwitcher.ShiftResult value)
        {
            result=value;feedbackUntil=Time.unscaledTime+.55f;var f=GameFlow.Instance;
            if(value==WorldSwitcher.ShiftResult.Accepted&&f!=null&&!f.player.IsGrounded&&f.player.transform.position.z>23)learnedAir=true;
        }
        void LateUpdate()
        {
            if(canvas==null || GameFlow.Instance==null)return;
            var flow=GameFlow.Instance;phone.Layout();scaler.scaleFactor=phone.Scale;
            Place(safe,new Rect(Screen.safeArea.x/phone.Scale,(Screen.height-Screen.safeArea.yMax)/phone.Scale,phone.Width,720));
            worldText.text=flow.worlds.IsAltered?"OVERGROWN":"PRESENT";
            Color worldColor=flow.worlds.IsAltered?cyan:amber;worldText.color=worldColor;
            timeText.text=PremiumHUD.FormatTime(flow.Elapsed);Place((RectTransform)timeText.transform.parent,new Rect(phone.Width-133,20,113,49));
            controls.SetActive(phone.Visible&&flow.IsPlaying);menu.SetActive(!flow.IsPlaying);
            Place(stick.rectTransform,phone.Router.Stick);Place(jump.rectTransform,phone.Router.Jump);Place(shift.rectTransform,phone.Router.Shift);
            StretchLabel(stick);StretchLabel(jump);StretchLabel(shift);Place(pause.rectTransform,phone.PauseRect);
            Vector2 point=phone.Router.Stick.center+new Vector2(phone.Router.Move.x,-phone.Router.Move.y)*phone.Router.Stick.width*.25f;
            Place(thumb.rectTransform,new Rect(point.x-20,point.y-20,40,40));jumpText.text=phone.Router.JumpHeld?"HOLD":"JUMP";
            jump.color=phone.Router.JumpHeld?amber:new Color(.85f,.94f,.94f,.38f);
            shift.color=!flow.worlds.IsReady?new Color(.66f,.72f,.75f,.22f):worldColor*.8f;
            if(Time.unscaledTime<feedbackUntil)shift.color=result==WorldSwitcher.ShiftResult.Blocked?new Color(1,.40f,.31f):result==WorldSwitcher.ShiftResult.Accepted?Color.white:amber;
            shiftText.color=flow.worlds.IsReady?Color.white:new Color(.7f,.75f,.77f);
            chord.gameObject.SetActive(phone.Router.JumpHeld);
            Vector2 a=phone.Router.Jump.center,b=phone.Router.Shift.center;
            Place(chord.rectTransform,new Rect((a.x+b.x)*.5f,(a.y+b.y)*.5f,Vector2.Distance(a,b)*.45f,2));
            chord.rectTransform.pivot=new Vector2(.5f,.5f);chord.rectTransform.localRotation=Quaternion.Euler(0,0,-Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg);
            string guidance="";float z=flow.player.transform.position.z;
            if(flow.HasShiftBridgeGuidance)
            {
                if(!bridgeSeen){bridgeSeen=true;lessonUntil=Time.unscaledTime+6;}
                if(!flow.worlds.IsAltered)guidance=phone.Visible?"SHIFT makes outlined roofs solid":"SHIFT / X makes outlined roofs solid";
                else if(Time.unscaledTime<lessonUntil)guidance="Bridge solid. Jump and keep moving.";
            }
            else if(z>20&&z<29&&!learnedAir)guidance=phone.Visible?"Hold JUMP, then slide your thumb to SHIFT":"Jump, then Shift while airborne";
            else if(flow.Elapsed<4)guidance=phone.Visible?"Left thumb moves · drag the right side to look":"WASD move · Space jump · Shift change world";
            hintBox.gameObject.SetActive(flow.IsPlaying&&guidance.Length>0);hint.text=guidance;Place(hintBox.rectTransform,new Rect(phone.Width*.5f-220,607,440,42));
            string message=flow.ActiveNotice;
            if(message=="OVERGROWN WORLD"||message=="PRESENT WORLD")message="";
            if(message=="SHIFT BLOCKED")message="SHIFT BLOCKED · move clear of the obstacle";
            if(message=="TRY AGAIN")message="BACK AT CHECKPOINT";
            noticeBox.gameObject.SetActive(flow.IsPlaying&&message.Length>0);notice.text=message;Place(noticeBox.rectTransform,new Rect(phone.Width*.5f-195,553,390,40));
            if(!flow.IsPlaying)UpdateMenu(flow);
        }
        void UpdateMenu(GameFlow flow)
        {
            menuTitle.text=flow.IsComplete?"DELIVERY COMPLETE":"PAUSED";Place(menuTitle.rectTransform,new Rect(phone.Width*.5f-250,80,500,42));
            Place(menuBox.rectTransform,new Rect(phone.Width*.5f-270,68,540,flow.IsComplete?284:phone.ShowDiagnostics?626:576));
            string[] text={flow.IsComplete?"YOUR TIME   "+PremiumHUD.FormatTime(flow.Elapsed):"RESUME RUN",flow.IsComplete?"RUN AGAIN":"RESTART FROM BEGINNING",
                "Control size","Horizontal inset","Control height","Camera sensitivity","Volume", "Camera follow   "+(PlayerPreferences.CameraAssist?"ON":"OFF"),
                "Invert vertical   "+(PlayerPreferences.InvertY?"ON":"OFF"),PlayerPreferences.LowPower?"Battery mode   30 FPS":"Smooth mode   60 FPS","QA frame recording"};
            float[] values={Mathf.InverseLerp(.8f,1.35f,PlayerPreferences.ControlScale),PlayerPreferences.ControlInset/90,PlayerPreferences.ControlHeight/100,Mathf.InverseLerp(.3f,2.5f,PlayerPreferences.TouchSensitivity),PlayerPreferences.Volume};
            for(int row=0;row<11;row++)
            {
                rows[row].transform.parent.gameObject.SetActive(flow.IsComplete?row<4:row<10||phone.ShowDiagnostics);Place((RectTransform)rows[row].transform.parent,phone.MenuRect(row));
                rows[row].text=flow.IsComplete&&row==2?"Two worlds. One route.":flow.IsComplete&&row==3?"Try a different line on your next run.":text[row];
                if(row>=2&&row<=6)
                {
                    tracks[row-2].gameObject.SetActive(!flow.IsComplete);fills[row-2].gameObject.SetActive(!flow.IsComplete);
                    Place(rows[row].rectTransform,new Rect(12,0,flow.IsComplete?476:185,46));Place(fills[row-2].rectTransform,new Rect(210,19,Mathf.Max(8,275*values[row-2]),8));
                }
            }
        }
        Sprite Shape(int kind)
        {
            const int n=64;var t=new Texture2D(n,n,TextureFormat.RGBA32,false);t.filterMode=FilterMode.Bilinear;
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {
                float d=Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f));
                float alpha=kind==0?Mathf.Clamp01(31.5f-d):kind==1?Mathf.Clamp01(31.5f-d)*Mathf.Clamp01(d-28.5f):
                    Mathf.Clamp01(8.5f-Vector2.Distance(new Vector2(x,y),new Vector2(Mathf.Clamp(x,8,55),Mathf.Clamp(y,8,55))));t.SetPixel(x,y,new Color(1,1,1,alpha));
            }
            t.Apply();return Sprite.Create(t,new Rect(0,0,n,n),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,kind==2?new Vector4(9,9,9,9):Vector4.zero);
        }
        static RectTransform Area(Transform parent,string name,Rect area){var o=new GameObject(name,typeof(RectTransform));o.transform.SetParent(parent,false);var r=o.GetComponent<RectTransform>();Place(r,area);return r;}
        static void Place(RectTransform r,Rect a){r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(a.x,-a.y);r.sizeDelta=a.size;}
        Image Box(Transform p,string n,Rect a){var i=Area(p,n,a).gameObject.AddComponent<Image>();i.sprite=rounded;i.type=Image.Type.Sliced;i.color=new Color(.035f,.066f,.084f,.83f);i.raycastTarget=false;return i;}
        Image Disc(Transform p,string n,Rect a,bool stroke){var i=Box(p,n,a);i.type=Image.Type.Simple;i.sprite=stroke?ring:disc;i.color=new Color(.85f,.94f,.94f,.38f);return i;}
        Text Label(Transform p,string t,Rect a,int size,TextAnchor align){var l=Area(p,"Label",a).gameObject.AddComponent<Text>();l.font=font;l.text=t;l.fontSize=size;l.fontStyle=FontStyle.Bold;l.color=Color.white;l.alignment=align;l.horizontalOverflow=HorizontalWrapMode.Wrap;l.raycastTarget=false;return l;}
        static void StretchLabel(Image i){if(i.transform.childCount>0)Place((RectTransform)i.transform.GetChild(0),new Rect(Vector2.zero,i.rectTransform.sizeDelta));}
        void OnDestroy(){if(GameFlow.Instance!=null)GameFlow.Instance.worlds.ShiftAttempted-=OnShift;foreach(var s in new[]{disc,ring,rounded})if(s!=null){Destroy(s.texture);Destroy(s);}}
    }
}
