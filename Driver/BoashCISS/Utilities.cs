using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoschCISS
{
    public class Utilities
    {
        public static short GetShortData(ref byte[] data, int bitIndex, int bitSize)
        {
            int byteSize = 0;

            if ((bitIndex + bitSize) / 8 - bitIndex / 8 <= 1)
            {
                byteSize = 1;
            }
            else
            {
                byteSize = 2;
            }

            ushort[] read = new ushort[byteSize];
            int[] bitLengh = new int[byteSize];

            Buffer.BlockCopy(data, bitIndex / 8, read, 0, byteSize * 2);

            if (!(bitSize / 8 == 1 && bitSize % 8 == 0))
            {
                for (int k = 0; k < byteSize; k++)
                {
                    ushort t = 0;

                    t = (ushort)(read[k] >> 8);
                    t += (ushort)((read[k] & 0xff) << 8);

                    read[k] = t;
                } 
            }

            if (bitSize % 8 > 0)
            {
                read[0] = (ushort)(read[0] >> bitSize % 8);
            }

            if (byteSize == 1 || bitIndex % 16 == 0)
            {
                bitLengh[0] = bitSize;
            }
            else
            {
                bitLengh[0] = 16 - bitIndex % 16;
            }

            if (byteSize == 2)
            {
                bitLengh[1] = bitSize - bitLengh[0];
            }

            bool minus = false;

            //Byte Order 0
            ushort first = 0;
            ushort mask = 0;

            for (int i = 0; i < 16; i++)
            {
                if ((16 - i) <= bitLengh[0])
                {
                    if ((16 - i) == bitLengh[0])
                    {
                        minus = (read[0] & (ushort)(1 << bitLengh[0] - 1)) > 0;
                    }
                    else
                    {
                        if ((16 - i) < bitLengh[0])
                        {
                            mask += (ushort)(1 << (16 - i - 1));
                        }
                    }
                }
            }

            first += (ushort)(read[0] & mask);


            //Byte Order 1
            ushort second = 0;
            ushort mask2 = 0;
            if (byteSize == 2)
            {
                mask2 = 0;

                for (int i = 0; i < 16; i++)
                {
                    if (i < bitLengh[1])
                    {
                        mask2 += (ushort)(1 << i);
                    }
                    else
                    {
                        mask2 = (ushort)(mask2 << 1);
                    }
                }

                second += (ushort)(read[1] & mask2);
                second = (ushort)(second >> (16 - bitLengh[1]));
            }

            //Sum
            int temp = 0;

            if (minus)
            {
                int fill = 0;
                int index = 0;

                index = bitLengh[0];

                if (byteSize == 2)
                {
                    index += bitLengh[1];
                }

                int j = 0;

                for (; j <= 16 - index; j++)
                {
                    fill |= 1 << j;
                }

                temp |= fill << (16 - j);
            }

            if (byteSize == 1)
            {
                temp |= (first & mask);
            }
            else
            {
                temp |= (first & mask) << bitLengh[1];
            }

            short result = 0;

            result |= (short)(temp);
            result |= (short)second;

            return result;
        }

        public static ushort GetuShortData(ref byte[] data, int bitIndex, int bitSize)
        {
            int byteSize = 0;

            if (bitSize <= 16 - (bitIndex % 8))
            {
                byteSize = 1;
            }
            else
            {
                byteSize = 2;
            }

            ushort[] read = new ushort[byteSize];
            int[] bitLengh = new int[byteSize];

            Buffer.BlockCopy(data, bitIndex / 8, read, 0, byteSize * 2);

            for (int k = 0; k < byteSize; k++)
            {
                ushort t = 0;

                t = (ushort)(read[k] >> 8);
                t += (ushort)((read[k] & 0xff) << 8);

                read[k] = t;
            }

            bitLengh[0] = 16 - bitIndex % 16;

            if (byteSize == 2)
            {
                bitLengh[1] = bitSize - bitLengh[0];
            }

            bool minus = false;

            //Byte Order 0
            ushort first = 0;
            ushort mask = 0;

            for (int i = 0; i < 16; i++)
            {
                if ((16 - i) <= bitLengh[0])
                {
                    if ((16 - i) == bitLengh[0])
                    {
                        minus = (read[0] & (ushort)(1 << bitLengh[0] - 1)) > 0;
                    }
                    else
                    {
                        if ((16 - i) < bitLengh[0])
                        {
                            mask += (ushort)(1 << (16 - i - 1));
                        }
                    }
                }
            }

            first += (ushort)(read[0] & mask);


            //Byte Order 1
            ushort second = 0;
            ushort mask2 = 0;
            if (byteSize == 2)
            {
                mask2 = 0;

                for (int i = 0; i < 16; i++)
                {
                    if (i < bitLengh[1])
                    {
                        mask2 += (ushort)(1 << i);
                    }
                    else
                    {
                        mask2 = (ushort)(mask2 << 1);
                    }
                }

                second += (ushort)(read[1] & mask2);
                second = (ushort)(second >> (16 - bitLengh[1]));
            }

            //Sum
            int temp = 0;

            if (minus)
            {
                int fill = 0;
                int index = 0;

                index = bitLengh[0];

                if (byteSize == 2)
                {
                    index += bitLengh[1];
                }

                int j = 0;

                for (; j <= 16 - index; j++)
                {
                    fill |= 1 << j;
                }

                temp |= fill << (16 - j);
            }

            if (byteSize == 1)
            {
                temp |= (first & mask);
            }
            else
            {
                temp |= (first & mask) << bitLengh[1];
            }

            ushort result = 0;

            result |= (ushort)(temp);
            result |= (ushort)second;

            return result;
        }

        public static int GetIntData(ref byte[] data, int byteIndex)
        {
            byte[] read = new byte[4];

            Buffer.BlockCopy(data, byteIndex, read, 0, 4);

            Array.Reverse(read);

            int[] result = new int[1];

            Buffer.BlockCopy(read, 0, result, 0, 4);

            return result[0];
        }
    }
}
