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
        Image stick, jump, shift, thumb, pause, hintBox, noticeBox;
        Text worldText, timeText, jumpText, hint, notice;
        GameObject controls;
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
            hintBox=Box(safe,"Hint",new Rect(phone.Width*.5f-285,104,570,86));
            hint=Label(hintBox.transform,"",new Rect(12,5,546,76),18,TextAnchor.MiddleCenter);
            noticeBox=Box(safe,"Notice",new Rect(phone.Width*.5f-210,202,420,48));
            notice=Label(noticeBox.transform,"",new Rect(10,3,400,42),22,TextAnchor.MiddleCenter);
        }
        void LateUpdate()
        {
            if(canvas==null || GameFlow.Instance==null)return;
            var flow=GameFlow.Instance; phone.Layout(); scaler.scaleFactor=phone.Scale;
            Place(safe,new Rect(Screen.safeArea.x/phone.Scale,(Screen.height-Screen.safeArea.yMax)/phone.Scale,phone.Width,720));
            worldText.text=flow.worlds.IsAltered?"OVERGROWN":"PRESENT"; timeText.text=PremiumHUD.FormatTime(flow.Elapsed);
            Place((RectTransform)timeText.transform.parent,new Rect(phone.Width-167,20,147,58));
            controls.SetActive(flow.IsPlaying);
            Place(stick.rectTransform,phone.Router.Stick); Place(jump.rectTransform,phone.Router.Jump); Place(shift.rectTransform,phone.Router.Shift);
            StretchLabel(stick); StretchLabel(jump); StretchLabel(shift); Place(pause.rectTransform,phone.PauseRect);
            Vector2 point=phone.Router.Stick.center+new Vector2(phone.Router.Move.x,-phone.Router.Move.y)*phone.Router.Stick.width*.25f;
            Place(thumb.rectTransform,new Rect(point.x-22,point.y-22,44,44)); jumpText.text=phone.Router.JumpHeld?"HOLD":"JUMP";
            string guidance="";
            if(flow.HasShiftBridgeGuidance)
                guidance=flow.worlds.IsAltered?"BRIDGE SOLID — CROSS IN OVERGROWN\nMove forward and hold JUMP to jump across.":
                    "TAP SHIFT TO MAKE THE BRIDGE SOLID\nBlue previews cannot support you. Switch before jumping.";
            else if(flow.IsPlaying && !string.IsNullOrEmpty(flow.CheckpointHint))
                guidance=flow.CheckpointHint.Replace("SPACE","JUMP").Replace("Space","JUMP").Replace("press Shift","tap SHIFT");
            else if(flow.IsPlaying && flow.Elapsed<7f)
                guidance="Hold JUMP; slide onto SHIFT for midair change\nDrag the free right area to look.";
            hintBox.gameObject.SetActive(guidance.Length>0); hint.text=guidance;
            Place(hintBox.rectTransform,new Rect(phone.Width*.5f-285,104,570,86));
            noticeBox.gameObject.SetActive(!string.IsNullOrEmpty(flow.ActiveNotice)); notice.text=flow.ActiveNotice;
            Place(noticeBox.rectTransform,new Rect(phone.Width*.5f-210,202,420,48));
        }
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
            image.color=new Color(.035f,.085f,.12f,.87f); image.raycastTarget=false; return image;
        }
        Image Disc(Transform parent,string name,Rect area)
        {
            Image image=Box(parent,name,area); image.sprite=circle; image.color=new Color(.04f,.09f,.12f,.62f); return image;
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
