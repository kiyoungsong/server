using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;

namespace DriverInterface
{
	public static class Definition
    {
        public enum DataType
        {
            STRING = 1,
            BOOL = 1,
            CHAR = BOOL,
            SHORT = BOOL,
            USHORT = BOOL,
            FLOAT = 2,
            INT = FLOAT,
            UINT = FLOAT,
            DOUBLE = 4,
            LONG = DOUBLE,
            ULONG = DOUBLE,
        }

        public static readonly ushort[] CRCTable =
        {
            0X0000, 0XC0C1, 0XC181, 0X0140, 0XC301, 0X03C0, 0X0280, 0XC241, 0XC601, 0X06C0,
            0X0780, 0XC741, 0X0500, 0XC5C1, 0XC481, 0X0440, 0XCC01, 0X0CC0, 0X0D80, 0XCD41,
            0X0F00, 0XCFC1, 0XCE81, 0X0E40, 0X0A00, 0XCAC1, 0XCB81, 0X0B40, 0XC901, 0X09C0,
            0X0880, 0XC841, 0XD801, 0X18C0, 0X1980, 0XD941, 0X1B00, 0XDBC1, 0XDA81, 0X1A40,
            0X1E00, 0XDEC1, 0XDF81, 0X1F40, 0XDD01, 0X1DC0, 0X1C80, 0XDC41, 0X1400, 0XD4C1,
            0XD581, 0X1540, 0XD701, 0X17C0, 0X1680, 0XD641, 0XD201, 0X12C0, 0X1380, 0XD341,
            0X1100, 0XD1C1, 0XD081, 0X1040, 0XF001, 0X30C0, 0X3180, 0XF141, 0X3300, 0XF3C1,
            0XF281, 0X3240, 0X3600, 0XF6C1, 0XF781, 0X3740, 0XF501, 0X35C0, 0X3480, 0XF441,
            0X3C00, 0XFCC1, 0XFD81, 0X3D40, 0XFF01, 0X3FC0, 0X3E80, 0XFE41, 0XFA01, 0X3AC0,
            0X3B80, 0XFB41, 0X3900, 0XF9C1, 0XF881, 0X3840, 0X2800, 0XE8C1, 0XE981, 0X2940,
            0XEB01, 0X2BC0, 0X2A80, 0XEA41, 0XEE01, 0X2EC0, 0X2F80, 0XEF41, 0X2D00, 0XEDC1,
            0XEC81, 0X2C40, 0XE401, 0X24C0, 0X2580, 0XE541, 0X2700, 0XE7C1, 0XE681, 0X2640,
            0X2200, 0XE2C1, 0XE381, 0X2340, 0XE101, 0X21C0, 0X2080, 0XE041, 0XA001, 0X60C0,
            0X6180, 0XA141, 0X6300, 0XA3C1, 0XA281, 0X6240, 0X6600, 0XA6C1, 0XA781, 0X6740,
            0XA501, 0X65C0, 0X6480, 0XA441, 0X6C00, 0XACC1, 0XAD81, 0X6D40, 0XAF01, 0X6FC0,
            0X6E80, 0XAE41, 0XAA01, 0X6AC0, 0X6B80, 0XAB41, 0X6900, 0XA9C1, 0XA881, 0X6840,
            0X7800, 0XB8C1, 0XB981, 0X7940, 0XBB01, 0X7BC0, 0X7A80, 0XBA41, 0XBE01, 0X7EC0,
            0X7F80, 0XBF41, 0X7D00, 0XBDC1, 0XBC81, 0X7C40, 0XB401, 0X74C0, 0X7580, 0XB541,
            0X7700, 0XB7C1, 0XB681, 0X7640, 0X7200, 0XB2C1, 0XB381, 0X7340, 0XB101, 0X71C0,
            0X7080, 0XB041, 0X5000, 0X90C1, 0X9181, 0X5140, 0X9301, 0X53C0, 0X5280, 0X9241,
            0X9601, 0X56C0, 0X5780, 0X9741, 0X5500, 0X95C1, 0X9481, 0X5440, 0X9C01, 0X5CC0,
            0X5D80, 0X9D41, 0X5F00, 0X9FC1, 0X9E81, 0X5E40, 0X5A00, 0X9AC1, 0X9B81, 0X5B40,
            0X9901, 0X59C0, 0X5880, 0X9841, 0X8801, 0X48C0, 0X4980, 0X8941, 0X4B00, 0X8BC1,
            0X8A81, 0X4A40, 0X4E00, 0X8EC1, 0X8F81, 0X4F40, 0X8D01, 0X4DC0, 0X4C80, 0X8C41,
            0X4400, 0X84C1, 0X8581, 0X4540, 0X8701, 0X47C0, 0X4680, 0X8641, 0X8201, 0X42C0,
            0X4380, 0X8341, 0X4100, 0X81C1, 0X8081, 0X4040
        };


        public const byte READ_LENGTH = 0x16;
		public const int DATA_SIZE = 2044;
        
        public const int BYTE_READ_MAXSIZE = 1022;
        public const int DATA_MAX_SIZE = 4096;

		public const int NUM_COIL = 0x01;
		public const int NUM_INPUT = 0x02;
		public const int NUM_HOLDINGREGISTER = 0x03;
		public const int NUM_INPUTREGISTER = 0x04;


        public const string STRING = "string";
        public const string BOOL = "bool";
		public const string SHORT = "short";
        public const string USHORT = "ushort";
        public const string INT = "int";
        public const string UINT = "uint";
        public const string FLOAT = "float";
		public const string LONG = "long";
		public const string ULONG = "ulong";
        public const string DOUBLE = "double";


        public const string DllExtension = ".dll";
        public const string DRIVER = "driver";
        public const string CONFIG = "config";
        public const string RESOURCE = "resource";
        public const string DeviceInfoName = "ChannelInfo.xml";
        public const string UIConfigName = "UIConfig.xml";
        public const string TEXT = ".txt";
        public const string WRITEFILE = "_WR";


        public const int MAX_MELSEC_BIT_SIZE = 1536;
        public const int MAX_MELSEC_WORD_SIZE = 2048;
        public const int MELSEC_BIT_UNIT = 8;

        //data = byte[] { 0, 1 } - short (2byte)
        public static void GetTypeValue(bool bswap, int size, Type type, ref object data)
        {
            if (data is byte[] bytes)
            {
                switch (type)
                {
                    case Type t when t == typeof(bool):
                        if (size > 1)
                        {
                            bool[] bArr = new bool[size];
                            
                            for (int i = 0; i < bytes.Length / 2; i++)
                            {
                                byte[] byteArr = ByteSwap(bytes.ToList().GetRange(i * 2, 2).ToArray(), bswap);

                                bArr[i] = BitConverter.ToBoolean(byteArr, 0);
                            }
                            
                            data = bArr;
                        }
                        else
                        {
                            bytes = ByteSwap(bytes, bswap);
                            data = BitConverter.ToBoolean(bytes, 0);
                        }
                        break;
                    case Type t1 when t1 == typeof(string):
                        data = ConvertToString(bytes);
                        break;
                    case Type t when t == typeof(short):
                        if (size > 1)
                        {
                            short[] bArr = new short[size];
                            for (int i = 0; i < bytes.Length / 2; i++)
                            {
                                byte[] byteArr = ByteSwap(bytes.ToList().GetRange(i * 2, 2).ToArray(), bswap);

                                bArr[i] = BitConverter.ToInt16(byteArr, 0);
                            }
                            data = bArr;
                        }
                        else
                        {
                            bytes = ByteSwap(bytes, bswap);

                            data = BitConverter.ToInt16(bytes, 0);
                        }
                        break;
                    case Type t when t == typeof(int):
                        if (size > 1)
                        {
                            int[] bArr = new int[size];
                            for (int i = 0; i < bytes.Length / 4; i++)
                            {
                                byte[] byteArr = ByteSwap(bytes.ToList().GetRange(i * 4, 4).ToArray(), bswap);

                                bArr[i] = BitConverter.ToInt32(byteArr, 0);
                            }
                            data = bArr;
                        }
                        else
                        {
                            bytes = ByteSwap(bytes, bswap);
                            
                            data = BitConverter.ToInt32(bytes, 0);
                        }
                        break;
                    case Type t when t == typeof(long):
                        if (size > 1)
                        {
                            long[] bArr = new long[size];
                            for (int i = 0; i < bytes.Length / 8; i++)
                            {
                                byte[] byteArr = ByteSwap(bytes.ToList().GetRange(i * 8, 8).ToArray(), bswap);

                                bArr[i] = BitConverter.ToInt64(byteArr, 0);
                            }
                            data = bArr;
                        }
                        else
                        {
                            bytes = ByteSwap(bytes, bswap);

                            data = BitConverter.ToInt64(bytes, 0);
                        }
                        break;
                    case Type t when t == typeof(float):
                        if (size > 1)
                        {
                            float[] bArr = new float[size];
                            for (int i = 0; i < bytes.Length / 4; i++)
                            {
                                byte[] byteArr = ByteSwap(bytes.ToList().GetRange(i * 4, 4).ToArray(), bswap);

                                bArr[i] = BitConverter.ToSingle(byteArr, 0);
                            }
                            data = bArr;
                        }
                        else
                        {
                            bytes = ByteSwap(bytes, bswap);

                            data = BitConverter.ToSingle(bytes, 0);
                        }
                        break;
                    case Type t when t == typeof(double):
                        if (size > 1)
                        {
                            double[] bArr = new double[size];
                            for (int i = 0; i < bytes.Length / 8; i++)
                            {
                                byte[] byteArr = ByteSwap(bytes.ToList().GetRange(i * 8, 8).ToArray(), bswap);

                                bArr[i] = BitConverter.ToDouble(byteArr, 0);
                            }
                            data = bArr;
                        }
                        else
                        {
                            bytes = ByteSwap(bytes, bswap);

                            data = BitConverter.ToDouble(bytes, 0);
                        }
                        break;
                    case Type t when t == typeof(ushort):
                        if (size > 1)
                        {
                            ushort[] bArr = new ushort[size];
                            for (int i = 0; i < bytes.Length / 2; i++)
                            {
                                byte[] byteArr = ByteSwap(bytes.ToList().GetRange(i * 2, 2).ToArray(), bswap);
                                
                                bArr[i] = BitConverter.ToUInt16(byteArr, 0);
                            }
                            data = bArr;
                            
                        }
                        else
                        {
                            bytes = ByteSwap(bytes, bswap);
                            data = BitConverter.ToUInt16(bytes, 0);
                        }
                        break;
                    case Type t when t == typeof(uint):
                        if (size > 1)
                        {
                            
                            uint[] bArr = new uint[size];
                            for (int i = 0; i < bytes.Length / 4; i++)
                            {
                                
                                byte[] byteArr = ByteSwap(bytes.ToList().GetRange(i * 4, 4).ToArray(), bswap);
                                
                                bArr[i] = BitConverter.ToUInt32(byteArr, 0);
                            }
                            data = bArr;
                            
                        }
                        else
                        {
                            bytes = ByteSwap(bytes, bswap);
                            data = BitConverter.ToUInt32(bytes, 0);
                        }
                        break;
                    case Type t when t == typeof(ulong):
                        if (size > 1)
                        {
                            ulong[] bArr = new ulong[size];
                            for (int i = 0; i < bytes.Length / 8; i++)
                            {
                                byte[] byteArr = ByteSwap(bytes.ToList().GetRange(i * 8, 8).ToArray(), bswap);

                                bArr[i] = BitConverter.ToUInt64(byteArr, 0);
                            }
                            data = bArr;
                        }
                        else
                        {
                            bytes = ByteSwap(bytes, bswap);

                            data = BitConverter.ToUInt64(bytes, 0);
                        }
                        break;
                }
            }
        }

        public static object ObjectToBytes(bool bswap, int size, Type type, object data)
        {
            object returnValue = null;
            
            switch (type)
            {
                case Type t when t == typeof(bool):
                    if (size > 1)
                    {
                        List<byte> byteList = new List<byte>();
                        foreach (var arr in (bool[])data)
                        {
                            byte[] byteArr = BitConverter.GetBytes(arr);
                            foreach (var byteVar in byteArr)
                            {
                                byteList.Add(byteVar);
                                byteList.Add(0);
                            }
                        }
                        returnValue = ByteSwap(byteList.ToArray(), bswap);
                    }
                    else
                    {
                        byte[] byteArr = BitConverter.GetBytes(Convert.ToBoolean(data));
                        List<byte> temp = byteArr.ToList();
                        temp.Add(0);
                        byteArr = temp.ToArray();
                        returnValue = ByteSwap(byteArr, bswap);
                    }
                    break;
                case Type t1 when t1 == typeof(string):
                    returnValue = StringToRegister((string)data, size);
                    break;
                case Type t when t == typeof(short):
                    if (size > 1)
                    {
                        List<byte> byteList = new List<byte>();
                        foreach (var arr in (short[])data)
                        {
                            byte[] byteArr = BitConverter.GetBytes(arr);
                            foreach (var byteVar in byteArr)
                            {
                                byteList.Add(byteVar);
                            }
                        }
                        returnValue = ByteSwap(byteList.ToArray(), bswap);
                    }
                    else
                    {
                        byte[] byteArr = BitConverter.GetBytes(Convert.ToInt16(data));
                        returnValue = ByteSwap(byteArr, bswap);
                    }
                    break;
                case Type t when t == typeof(int):
                    if (size > 1)
                    {
                        List<byte> byteList = new List<byte>();
                        foreach (var arr in (int[])data)
                        {
                            byte[] byteArr = BitConverter.GetBytes(arr);
                            foreach (var byteVar in byteArr)
                            {
                                byteList.Add(byteVar);
                            }
                        }
                        returnValue = ByteSwap(byteList.ToArray(), bswap);
                    }
                    else
                    {
                        byte[] byteArr = BitConverter.GetBytes(Convert.ToInt32(data));
                        returnValue = ByteSwap(byteArr, bswap);
                    }
                    break;
                case Type t when t == typeof(long):
                    if (size > 1)
                    {
                        List<byte> byteList = new List<byte>();
                        foreach (var arr in (long[])data)
                        {
                            byte[] byteArr = BitConverter.GetBytes(arr);
                            foreach (var byteVar in byteArr)
                            {
                                byteList.Add(byteVar);
                            }
                        }
                        returnValue = ByteSwap(byteList.ToArray(), bswap);
                    }
                    else
                    {
                        byte[] byteArr = BitConverter.GetBytes(Convert.ToInt64(data));
                        returnValue = ByteSwap(byteArr, bswap);
                    }
                    break;
                case Type t when t == typeof(float):
                    if (size > 1)
                    {
                        List<byte> byteList = new List<byte>();
                        foreach (var arr in (float[])data)
                        {
                            byte[] byteArr = BitConverter.GetBytes(arr);
                            foreach (var byteVar in byteArr)
                            {
                                byteList.Add(byteVar);
                            }
                        }
                        returnValue = ByteSwap(byteList.ToArray(), bswap);
                    }
                    else
                    {
                        byte[] byteArr = BitConverter.GetBytes(Convert.ToSingle(data));
                        returnValue = ByteSwap(byteArr, bswap);
                    }
                    break;
                case Type t when t == typeof(double):
                    if (size > 1)
                    {
                        List<byte> byteList = new List<byte>();
                        foreach (var arr in (double[])data)
                        {
                            byte[] byteArr = BitConverter.GetBytes(arr);
                            foreach (var byteVar in byteArr)
                            {
                                byteList.Add(byteVar);
                            }
                        }
                        returnValue = ByteSwap(byteList.ToArray(), bswap);
                    }
                    else
                    {
                        byte[] byteArr = BitConverter.GetBytes(Convert.ToDouble(data));
                        returnValue = ByteSwap(byteArr, bswap);
                    }
                    break;
                case Type t when t == typeof(ushort):
                    if (size > 1)
                    {
                        List<byte> byteList = new List<byte>();
                        foreach (var arr in (ushort[])data)
                        {
                            byte[] byteArr = BitConverter.GetBytes(arr);
                            foreach (var byteVar in byteArr)
                            {
                                byteList.Add(byteVar);
                            }
                        }
                        returnValue = ByteSwap(byteList.ToArray(), bswap);
                    }
                    else
                    {
                        byte[] byteArr = BitConverter.GetBytes(Convert.ToUInt16(data));
                        returnValue = ByteSwap(byteArr, bswap);
                    }
                    break;
                case Type t when t == typeof(uint):
                    if (size > 1)
                    {
                        List<byte> byteList = new List<byte>();
                        foreach (var arr in (uint[])data)
                        {
                            byte[] byteArr = BitConverter.GetBytes(arr);
                            foreach (var byteVar in byteArr)
                            {
                                byteList.Add(byteVar);
                            }
                        }
                        returnValue = ByteSwap(byteList.ToArray(), bswap);
                    }
                    else
                    {
                        byte[] byteArr = BitConverter.GetBytes(Convert.ToUInt32(data));
                        returnValue = ByteSwap(byteArr, bswap);
                    }
                    break;
                case Type t when t == typeof(ulong):
                    if (size > 1)
                    {
                        List<byte> byteList = new List<byte>();
                        foreach (var arr in (ulong[])data)
                        {
                            byte[] byteArr = BitConverter.GetBytes(arr);
                            foreach (var byteVar in byteArr)
                            {
                                byteList.Add(byteVar);
                            }
                        }
                        returnValue = ByteSwap(byteList.ToArray(), bswap);
                    }
                    else
                    {
                        byte[] byteArr = BitConverter.GetBytes(Convert.ToUInt64(data));
                        returnValue = ByteSwap(byteArr, bswap);
                    }
                    break;
            }

            return returnValue;
        }

        private static byte[] StringToRegister(string str, int size)
        {
            byte[] array = new byte[size * 2];
            byte[] strArray = System.Text.Encoding.ASCII.GetBytes(str);

            if (strArray.Length > array.Length)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = strArray[i];
                }
            }
            else
            {
                for (int i = 0; i < strArray.Length; i++)
                {
                    array[i] = strArray[i];
                }
            }
            
            return array;
        }

        private static object ConvertToString(byte[] bytes)
        {
            object data;
            byte[] result = new byte[bytes.Length];

            for (int i = 0; i < bytes.Length; i++)
            {
                result[i] = bytes[i];
            }
            data = System.Text.Encoding.ASCII.GetString(result);
            return data;
        }

        private static byte[] ByteSwap(byte[] bytes, bool swap)
        {
            if (swap)
            {
                if (bytes.Length % 2 == 0)
                {
                    for (int i = 0; i < bytes.Length / 2; i++)
                    {
                        byte tempByte = bytes[i];
                        bytes[i * 2] = bytes[(i * 2) + 1];
                        bytes[(i * 2) + 1] = tempByte;
                    }
                }
                else
                {
                    throw new Exception("Byte Swap Func byte Array Length Not Matched");
                }
            }

            return bytes;
        }

        public static int HexToDec(string str)
        {
            string strHex = string.Format("{0:X6}", str.ToUpper());
            int address = Convert.ToInt32(strHex, 16);

            return address;
        }
    }
}