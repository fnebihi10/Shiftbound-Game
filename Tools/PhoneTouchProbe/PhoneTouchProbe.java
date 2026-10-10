package com.shiftboundproject.shiftbound.qa;

import android.app.Instrumentation;
import android.app.UiAutomation;
import android.content.Intent;
import android.content.ComponentName;
import android.graphics.Bitmap;
import android.os.Bundle;
import android.os.SystemClock;
import android.view.InputDevice;
import android.view.MotionEvent;
import java.io.File;
import java.io.FileOutputStream;
import java.io.PrintWriter;

// A separate opt-in test APK: Android's real two-pointer delivery, no root,
// game code injection, permissions, networking or production dependencies.
// Run only with the owner leaving the screen untouched. Evidence is synthetic
// OS input, not a physical input-to-photon or thumb-comfort measurement.
public final class PhoneTouchProbe extends Instrumentation {
    private UiAutomation ui;
    private Bundle options;
    private File output;
    private PrintWriter log;
    private long down;
    private float scale, left, width, controlSize, controlInset, controlHeight;
    private float[] sx = new float[2], sy = new float[2];
    private int count;
    private int touchDevice;

    @Override public void onCreate(Bundle args) { options = args; super.onCreate(args); start(); }
    @Override public void onStart() {
        Bundle result = new Bundle();
        try {
            ui = getUiAutomation();
            for(int id:InputDevice.getDeviceIds()) {
                InputDevice device=InputDevice.getDevice(id);
                if(device!=null&&!device.isVirtual()&&device.supportsSource(InputDevice.SOURCE_TOUCHSCREEN)
                    &&device.getName().toLowerCase(java.util.Locale.ROOT).contains("touchscreen")) {touchDevice=id;break;}
            }
            if(touchDevice==0)throw new IllegalStateException("No physical Android touchscreen descriptor");
            output = new File(getContext().getExternalFilesDir(null), "TouchProbe");
            output.mkdirs(); log = new PrintWriter(new File(output, "events.txt"));
            // Exact values supplied by the host after checking this phone's
            // screenshot, safe area and saved control settings. Never pretend
            // hard-coded coordinates establish every device's layout acceptance.
            left = Float.parseFloat(options.getString("safeLeft", "107"));
            float screenWidth = Float.parseFloat(options.getString("width", "2280"));
            float screenHeight = Float.parseFloat(options.getString("height", "1080"));
            scale = screenHeight / 720f; width = (screenWidth - left) / scale;
            controlSize=Float.parseFloat(options.getString("controlSize","1"));
            controlInset=Float.parseFloat(options.getString("controlInset","0"));
            controlHeight=Float.parseFloat(options.getString("controlHeight","0"));
            log.println("Synthetic Android MotionEvents; full-screen capture overhead excluded from performance runs.");
            log.println("width="+screenWidth+" height="+screenHeight+" safeLeft="+left);
            log.println("touchDevice="+touchDevice+"; OS-injected events, not physical finger contacts");
            String scenario = options.getString("scenario", "interaction");
            if(options.getString("startPaused","false").equals("true")) {
                tap(width*.5f,149);waitMs(500);
            }
            capture("00-before");
            if (scenario.equals("interaction")) interactions();
            else if (scenario.equals("orbit")) orbit();
            else throw new IllegalArgumentException("Unknown scenario: " + scenario);
            result.putString("result", "DELIVERED: review screenshots/events; no automatic artistic or comfort PASS");
        } catch (Throwable error) {
            result.putString("error", error.toString());
            if (log != null) error.printStackTrace(log);
        } finally {
            if (count > 0) { try { event(MotionEvent.ACTION_CANCEL); } catch (Throwable ignored) {} }
            if (log != null) log.close();
            finish(result.containsKey("error") ? 1 : 0, result);
        }
    }
    private void interactions() throws Exception {
        // Starts PLAYING at a stationary rooftop, default control layout.
        float bottom=684-controlHeight;
        float moveX=34+controlInset+96*controlSize, moveY=bottom-96*controlSize;
        float jumpX=width-40-controlInset-62*controlSize, jumpY=bottom-62*controlSize;
        float shiftX=width-52-controlInset-170*controlSize, shiftY=bottom-114*controlSize;
        press(0, moveX, moveY); move(0, moveX, moveY-55, 140);
        waitMs(170); press(1, jumpX, jumpY); waitMs(160);
        log.println(SystemClock.uptimeMillis()+" stage moving-jump-held");
        move(1, shiftX, shiftY, 110); waitMs(80);
        log.println(SystemClock.uptimeMillis()+" stage airborne-shift-held");
        // Oscillating across the core cannot cause a second Shift for this jump.
        move(1, jumpX, jumpY, 80); move(1, shiftX, shiftY, 80);
        waitMs(100); log.println(SystemClock.uptimeMillis()+" stage held-chord-single-activation");
        event(MotionEvent.ACTION_CANCEL); count=0;
        waitMs(1000); capture("04-cancel-neutral-recovery");
        // Camera contact crosses action areas without acquiring their ownership.
        press(0, width*.70f, 330); move(0, shiftX, shiftY, 400);
        release(0); waitMs(350); capture("05-camera-ownership");
        tap(width-218,48); waitMs(250); capture("06-actual-pause");
        // Stationary menu contact on text, secondary contact over a settings row.
        // Primary ownership prevents the second contact from changing controls.
        press(0,width*.5f-225,126+2*51+23); waitMs(180);
        press(1,width*.5f+150,126+5*51+23); waitMs(650);
        release(1); release(0); capture("07-menu-two-contacts");
        tap(width*.5f,149); waitMs(300); capture("08-neutral-resume");
        tap(width-218,48); waitMs(200); capture("09-left-paused");
    }
    private void orbit() throws Exception {
        for(int i=0;i<8;i++) {
            press(0,width*.65f,330); move(0,width*.65f+120,330,450);
            release(0); waitMs(180); capture(String.format("orbit-%02d",i));
        }
        tap(width-218,48); waitMs(200); capture("orbit-left-paused");
    }
    private void capture(String name) throws Exception {
        Bitmap frame=ui.takeScreenshot();
        if(frame==null)throw new IllegalStateException("No screenshot: "+name);
        try(FileOutputStream stream=new FileOutputStream(new File(output,name+".png"))) {
            frame.compress(Bitmap.CompressFormat.PNG,100,stream);
        }
        frame.recycle();log.println(SystemClock.uptimeMillis()+" capture "+name);log.flush();
    }
    private void press(int index,float x,float y) {
        if(index!=count)throw new IllegalStateException("Pointer order");
        if(count==0)down=SystemClock.uptimeMillis();
        sx[index]=left+x*scale;sy[index]=y*scale;count++;
        event(index==0?MotionEvent.ACTION_DOWN:MotionEvent.ACTION_POINTER_DOWN|(index<<8));
    }
    private void release(int index) {
        if(index!=count-1)throw new IllegalStateException("Release reverse order");
        event(count==1?MotionEvent.ACTION_UP:MotionEvent.ACTION_POINTER_UP|(index<<8));count--;
    }
    private void move(int index,float x,float y,long duration) {
        float x0=sx[index],y0=sy[index],x1=left+x*scale,y1=y*scale;
        int steps=Math.max(1,(int)(duration/16));
        for(int i=1;i<=steps;i++) {
            sx[index]=x0+(x1-x0)*i/steps;sy[index]=y0+(y1-y0)*i/steps;
            event(MotionEvent.ACTION_MOVE);waitMs(duration/steps);
        }
    }
    private void tap(float x,float y) {press(0,x,y);waitMs(70);release(0);}
    private void event(int action) {
        android.view.accessibility.AccessibilityNodeInfo root=ui.getRootInActiveWindow();
        if(root==null)throw new IllegalStateException("Cannot verify focused window; no input sent");
        try {if(!"com.shiftboundproject.shiftbound".contentEquals(root.getPackageName()))
            throw new IllegalStateException("Shiftbound lost focus; no input sent");}
        finally {root.recycle();}
        MotionEvent.PointerProperties[] properties=new MotionEvent.PointerProperties[count];
        MotionEvent.PointerCoords[] coords=new MotionEvent.PointerCoords[count];
        for(int i=0;i<count;i++) {
            properties[i]=new MotionEvent.PointerProperties();properties[i].id=i;properties[i].toolType=MotionEvent.TOOL_TYPE_FINGER;
            coords[i]=new MotionEvent.PointerCoords();coords[i].x=sx[i];coords[i].y=sy[i];coords[i].pressure=1;coords[i].size=.04f;
        }
        MotionEvent event=MotionEvent.obtain(down,SystemClock.uptimeMillis(),action,count,properties,coords,0,0,1,1,touchDevice,0,InputDevice.SOURCE_TOUCHSCREEN,0);
        try {if(!ui.injectInputEvent(event,true))throw new IllegalStateException("Android rejected input");}
        finally {event.recycle();}
        log.println(SystemClock.uptimeMillis()+" action="+action+" pointers="+count+" x0="+sx[0]+" y0="+sy[0]);log.flush();
    }
    private static void waitMs(long ms) {SystemClock.sleep(ms);}
}
