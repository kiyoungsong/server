using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IronUtilites
{
	public class MemoryManager
	{
		public struct MemoryMap
		{
			public List<int> addrs;
			public List<int> lengths;
		}

        public static MemoryMap CreateMemoryMap(int[] addrs, int[] lengths, int maxSize)
        {
			MemoryMap map;

			map.addrs = new List<int>();
			map.lengths = new List<int>();

			Dictionary<int, int> addrLengthDic = new Dictionary<int, int>();

			using (MemoryStream memStream = new MemoryStream())
			{
				int prevBuffIdx = 0;

				for (int i = 0; i < addrs.Length; i++)
				{
					if (lengths[i] > 0)
					{
						addrLengthDic[addrs[i]] = lengths[i];

						prevBuffIdx += lengths[i] * 2;

						byte[] buffer = new byte[lengths[i]];

						for (int j = 0; j < buffer.Length; j++)
						{
							buffer[j] = 1;
						}

						memStream.Seek(addrs[i], SeekOrigin.Begin);
						memStream.Write(buffer, 0, lengths[i]);
					}
				}

				memStream.Seek(0, SeekOrigin.Begin);

				int sectionAddr = -1;
				int sectionLength = 0;
				int partAddr = -1;
				int partLength = 0;
				int length = 0;
				int pos = 0;
				int value = 0;

				while (length < memStream.Length)
				{
					pos = Convert.ToInt32(memStream.Position);
					value = memStream.ReadByte();
					length++;

					if (value > 0)
					{
						if (partAddr < 0)
						{
							partAddr = pos;

							if (sectionAddr < 0)
							{
								sectionAddr = partAddr;
							}
						}
					}
					else
					{
						if (partAddr >= 0)
						{
							partLength = pos - partAddr;
							sectionLength = pos - sectionAddr;
							partAddr = -1;
						}
					}

					//max?
					if (sectionAddr >= 0 && pos - sectionAddr + 1 >= maxSize)
					{
						map.addrs.Add(sectionAddr);

						if (value > 0)
						{
							map.lengths.Add(maxSize);
						}
						else
						{
							map.lengths.Add(sectionLength);
						}

						partAddr = -1;
						sectionAddr = -1;
					}
				}

				if (sectionAddr >= 0)
				{
					map.addrs.Add(sectionAddr);

					if (value > 0)
					{
						map.lengths.Add(Convert.ToInt32(memStream.Length) - sectionAddr);
					}
					else
					{
						map.lengths.Add(sectionLength);
					}
				}
			}

			return map;
        }
    }
}
