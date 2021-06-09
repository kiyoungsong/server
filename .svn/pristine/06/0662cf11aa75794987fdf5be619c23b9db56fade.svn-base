using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronClient
{
    public class DynamicList<T>
    {
        List<T[]> dynamicList;
        public DynamicList()
        {
            dynamicList = new List<T[]>();
        }

        public List<T[]> Items => dynamicList;

        public int ArrayCount => dynamicList.Count;

        public void AddArray(T[] array)
        {
            dynamicList.Add(array);
        }

        public void Add(int index, T item)
        {
            if (dynamicList.Count <= index)
            {
                dynamicList.Add(new T[0]);
                Add(index, item);
                return;
            }
            T[] changeT = dynamicList[index];
            Array.Resize(ref changeT, changeT.Length + 1);
            changeT[changeT.Length - 1] = item;
            dynamicList[index] = changeT;
        }

        public int FindIndex(int index, T item)
        {
            T[] findArray = dynamicList[index];
            for (int i = 0; i < findArray.Length; i++)
            {
                if (findArray[i].ToString() == item.ToString())
                {
                    return i;
                }
            }
            IronUtilites.LogManager.Manager.WriteLog("IRONClient", "NotFound ClientHandler Number : " + item.ToString());

            return -1;
        }

        public void Remove(int index, T item)
        {
            if (index >= dynamicList.Count)
            {
                return;
            }
            int removeidx = FindIndex(index, item);
            for (int i = 0; i < dynamicList.Count; i++)
            {
                dynamicList[i] = dynamicList[i].Except(new T[] { dynamicList[i][removeidx]}).ToArray();
            }

        }

        public void RemoveAt(int removeidx)
        {

            for (int i = 0; i < dynamicList.Count; i++)
            {
                dynamicList[i] = dynamicList[i].Except(new T[] { dynamicList[i][removeidx] }).ToArray();
            }


            //if (removalIndex < 0 || removalIndex > _dynamicList.Length - 1) throw new IndexOutOfRangeException();
            //for (int i = removalIndex; i < _dynamicList.Length - 1; i++)
            //{
            //    _dynamicList[i] = _dynamicList[i + 1];
            //}
            //Array.Resize(ref _dynamicList, _dynamicList.Length - 1);
        }

        public void Clear()
        {
            dynamicList = new List<T[]>();
        }
    }
}
