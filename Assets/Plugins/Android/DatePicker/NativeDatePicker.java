package me.iboutsikas.NativeDialogs;
import android.util.Log;

public class NativeDatePicker {

    private static final NativeDatePicker s_Instance = new NativeDatePicker();
    private static final String LOGTAG = "NativeDatePicker";

    public static NativeDatePicker GetInstance() { return s_Instance; }

    private NativeDatePicker() {
        Log.i(LOGTAG, "Initialized native plugin");
    }

    public static String speak() {
        Log.i(LOGTAG, "Hi from the Java side.");
        return "Hi from the Java side.";
    }
}