﻿using System;
using System.Runtime.InteropServices;

/*
 * The C# API class for the BB series devices is a class of static members and methods which
 * are simply a 1-to-1 mapping of the C API. This makes is easy to modify and look up
 * functions in the API manual.
 */

enum bbStatus
{
    // Configuration Errors
    bbInvalidModeErr = -112,
    bbReferenceLevelErr = -111,
    bbInvalidVideoUnitsErr = -110,
    bbInvalidWindowErr = -109,
    bbInvalidBandwidthTypeErr = -108,
    bbInvalidSweepTimeErr = -107,
    bbBandwidthErr = -106,
    bbInvalidGainErr = -105,
    bbAttenuationErr = -104,
    bbFrequencyRangeErr = -103,
    bbInvalidSpanErr = -102,
    bbInvalidScaleErr = -101,
    bbInvalidDetectorErr = -100,

    // General Errors
    bbLibusbError = -18,
    bbNotSupportedErr = -17,
    bbTrackingGeneratorNotFound = -16,

    bbUSBTimeoutErr = -15,
    bbDeviceConnectionErr = -14,
    bbPacketFramingErr = -13,
    bbGPSErr = -12,
    bbGainNotSetErr = -11,
    bbDeviceNotIdleErr = -10,
    bbDeviceInvalidErr = -9,
    bbBufferTooSmallErr = -8,
    bbNullPtrErr = -7,
    bbAllocationLimitErr = -6,
    bbDeviceAlreadyStreamingErr = -5,
    bbInvalidParameterErr = -4,
    bbDeviceNotConfiguredErr = -3,
    bbDeviceNotStreamingErr = -2,
    bbDeviceNotOpenErr = -1,

    // No Error
    bbNoError = 0,

    // Warnings/Messages
    bbAdjustedParameter = 1,
    bbADCOverflow = 2,
    bbNoTriggerFound = 3,
    bbClampedToUpperLimit = 4,
    bbClampedToLowerLimit = 5,
    bbUncalibratedDevice = 6,
    bbDataBreak = 7,
    bbUncalSweep = 8
};

class bb_api
{
    public static int BB_TRUE = 1;
    public static int BB_FALSE = 0;
    // bbGetDeviceType : type
    public static int BB_DEVICE_NONE = 0;
    public static int BB_DEVICE_BB60A = 1;
    public static int BB_DEVICE_BB60C = 2;

    public static int BB_MAX_DEVICES = 8;
    // BB60A/C freq limits
    public static double BB60_MIN_FREQ = 9.0e3;
    public static double BB60_MAX_FREQ = 6.4e9;
    public static double BB60_MAX_SPAN = (BB60_MAX_FREQ - BB60_MIN_FREQ);
    // Frequencies specified in Hz
    public static double BB_MIN_SPAN = 20.0;
    public static double BB_MIN_BW = 0.602006912;
    public static double BB_MAX_BW = 10.1e6;
    public static double BB_MIN_RT_RBW = 2465.820313;
    public static double BB_MAX_RT_RBW = 631250.0;
    public static double BB_MIN_RT_SPAN = 200.0e3;
    public static double BB60A_MAX_RT_SPAN = 20.0e6;
    public static double BB60C_MAX_RT_SPAN = 27.0e6;
    public static double BB_MIN_SWEEP_TIME = 0.00001;
    public static double BB_MAX_SWEEP_TIME = 1.0;
    public static double BB_MIN_USB_VOLTAGE = 4.4;
    // bbConfigureLevel : atten
    public static double BB_AUTO_ATTEN = -1.0;
    public static double BB_MAX_REFERENCE = 50.0;
    public static double BB_MAX_ATTENUATION = 30.0;
    // bbConfigureIQ : downsampleFactor
    public static int BB_MIN_DECIMATION = 1; // 2 ^ 0
    public static int BB_MAX_DECIMATION = 8192; // 2 ^ 13
    // bbConfigureGain : gain
    public static int BB_AUTO_GAIN = -1;
    public static int BB60_MAX_GAIN = 3;
    public static int BB60C_MAX_GAIN = 3;
    // bbInitiate : mode
    public static uint BB_SWEEPING = 0x0;
    public static uint BB_REAL_TIME = 0x1;
    public static uint BB_STREAMING = 0x4;
    public static uint BB_AUDIO_DEMOD = 0x7;
    public static uint BB_TG_SWEEPING = 0x8;
    // bbConfigureSweepCoupling : rejection
    public static uint BB_NO_SPUR_REJECT = 0x0;
    public static uint BB_SPUR_REJECT = 0x1;
    // bbConfigAcquisition : scale
    public static uint BB_LOG_SCALE = 0x0;
    public static uint BB_LIN_SCALE = 0x1;
    public static uint BB_LOG_FULL_SCALE = 0x2;
    public static uint BB_LIN_FULL_SCALE = 0x3;
    // bbConfigureSweepCoupling : rbwShape
    public static uint BB_RBW_SHAPE_NUTTALL = 0x0;
    public static uint BB_RBW_SHAPE_FLATTOP = 0x1;
    public static uint BB_RBW_SHAPE_CISPR = 0x2;
    // bbConfigAcquisition : detector
    public static uint BB_MIN_AND_MAX = 0x0;
    public static uint BB_AVERAGE = 0x1;
    // bbConfigureProcUnits : units
    public static uint BB_LOG = 0x0;
    public static uint BB_VOLTAGE = 0x1;
    public static uint BB_POWER = 0x2;
    public static uint BB_SAMPLE = 0x3;
    // bbConfigureDemod: modulationType
    public static int BB_DEMOD_AM = 0x0;
    public static int BB_DEMOD_FM = 0x1;
    public static int BB_DEMOD_USB = 0x2;
    public static int BB_DEMOD_LSB = 0x3;
    public static int BB_DEMOD_CW = 0x4;
    // bbInitiate : flag
    public static uint BB_STREAM_IQ = 0x0;
    public static uint BB_STREAM_IF = 0x1;
    public static uint BB_DIRECT_RF = 0x2;
    public static uint BB_TIME_STAMP = 0x10;
    // bbConfigureIO : port1
    public static int BB_PORT1_AC_COUPLED = 0x00;
    public static int BB_PORT1_DC_COUPLED = 0x04;
    public static int BB_PORT1_INT_REF_OUT = 0x00;
    public static int BB_PORT1_EXT_REF_IN = 0x08;
    public static int BB_PORT1_OUT_AC_LOAD = 0x10;
    public static int BB_PORT1_OUT_LOGIC_LOW = 0x14;
    public static int BB_PORT1_OUT_LOGIC_HIGH = 0x1C;
    // bbConfigureIO : port2
    public static int BB_PORT2_OUT_LOGIC_LOW = 0x00;
    public static int BB_PORT2_OUT_LOGIC_HIGH = 0x20;
    public static int BB_PORT2_IN_TRIGGER_RISING_EDGE = 0x40;
    public static int BB_PORT2_IN_TRIGGER_FALLING_EDGE = 0x60;
    // bbStoreTgThru : flag
    public static int TG_THRU_0DB = 0x1;
    public static int TG_THRU_20DB = 0x2;
    // bbSetTgReference: reference
    public static int TG_REF_UNUSED = 0x0;
    public static int TG_REF_INTERNAL_OUT = 0x1;
    public static int TG_REF_EXTERNAL_IN = 0x2;

    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbGetSerialNumberList(int[] devices, ref int deviceCount);
    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbOpenDeviceBySerialNumber(ref int device, int serialNumber);

    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbOpenDevice(ref int device);
    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbCloseDevice(int device);

    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbConfigureAcquisition(int device,
        uint detector, uint scale);
    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbConfigureCenterSpan(int device,
        double center, double span);
    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbConfigureLevel(int device,
        double refLevel, double atten);
    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbConfigureGain(int device, int gain);
    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbConfigureSweepCoupling(int device,
        double rbw, double vbw, double sweepTime, uint rbwShape, uint rejection);
    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbConfigureProcUnits(int device, uint units);
    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbConfigureIO(int device,
        uint port1, uint port2);
    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbConfigureDemod(int device,
        int modType, double freq, float ifBandwidth, float lowPassFreq,
        float highPassFreq, float fmDeemphasis);
    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbConfigureIQ(int device,
        int downsampleFactor, double bandwidth);

    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbInitiate(int device, uint mode, uint flag);

    [DllImport("bb_api", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbFetchTrace_32f(int device,
        int arraySize, float[] min, float[] max);
    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbFetchTrace(int device,
        int arraysize, double[] min, double[] max);
    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbFetchRealTimeFrame(int device,
        float[] sweep_min, float[] sweep_max, float[] frame, float[] alphaFrame);
    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbFetchAudio(int device, float[] audio);
    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbGetIQUnpacked(int device, float[] iqData,
        int iqCount, int[] triggers, int triggerCount, int purge,
        ref int dataRemaining, ref int sampleLoss, ref int sec, ref int nano);

    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbQueryTraceInfo(int device,
        ref uint trace_len, ref double bin_size, ref double start);
    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbQueryStreamInfo(int device,
        ref int return_len, ref double bandwidth, ref int samples_per_sec);

    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbAbort(int device);
    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbPreset(int device);
    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbSelfCal(int device);
    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbSyncCPUtoGPS(int com_port, int baud_rate);

    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbGetDeviceType(int device, ref int type);
    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbGetSerialNumber(int device, ref uint serial_number);
    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbGetFirmwareVersion(int device, ref int version);
    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bbStatus bbGetDeviceDiagnostics(int device,
        ref float temperature, ref float usbVoltage, ref float usbCurrent);

    public static string bbGetDeviceName(int device)
    {
        int device_type = -1;
        bbGetDeviceType(device, ref device_type);
        if (device_type == BB_DEVICE_BB60A)
            return "BB60A";
        if (device_type == BB_DEVICE_BB60C)
            return "BB60C";

        return "Unknown device";
    }

    public static string bbGetSerialString(int device)
    {
        uint serial_number = 0;
        if (bbGetSerialNumber(device, ref serial_number) == bbStatus.bbNoError)
            return serial_number.ToString();

        return "";
    }

    public static string bbGetFirmwareString(int device)
    {
        int firmware_version = 0;
        if (bbGetFirmwareVersion(device, ref firmware_version) == bbStatus.bbNoError)
            return firmware_version.ToString();

        return "";
    }

    public static string bbGetAPIString()
    {
        IntPtr str_ptr = bbGetAPIVersion();
        return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(str_ptr);
    }

    public static string bbGetStatusString(bbStatus status)
    {
        IntPtr str_ptr = bbGetErrorString(status);
        return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(str_ptr);
    }

    // Call get_string variants above instead
    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr bbGetAPIVersion();
    [DllImport("bb_api.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr bbGetErrorString(bbStatus status);
}

