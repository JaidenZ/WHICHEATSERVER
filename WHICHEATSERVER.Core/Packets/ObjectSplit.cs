namespace WHICHEATSERVER.Core.Packets
{
    using WHICHEATSERVER.Core.Serialization;
    using WHICHEATSERVER.Core.Utilits;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using ssc;

    public static class ObjectSplit
    {
        /// <summary>
        /// 对象分割 每一包都是完整对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IList<T> Split<T>(this T obj, bool single = false) where T : class, new()
        {
            ISerializable objserializable = obj as ISerializable;
            if (objserializable == null)
            {
                return new List<T>() { obj };
            }


            Type objtype = obj.GetType();

            PropertyInfo[] proarry = objtype.GetProperties();

            byte[] objbuffer = objserializable.Serialize();

            if (objbuffer.Length < NBufferSplit.MAX_BUFFER_BLOCK_SIZE)
            {
                return new List<T>() { obj };
            }

            Dictionary<string, Type> needsolitfile = GetListTypes(proarry);

            if (needsolitfile == null || !needsolitfile.Any())
            {
                return new List<T>() { obj };
            }

            ////基础对象 不含列表
            T baseobj = GetBaseModel<T>(obj, objtype, proarry);
            objbuffer = (baseobj as ISerializable).Serialize();
            if (objbuffer.Length > NBufferSplit.MAX_BUFFER_BLOCK_SIZE)
            {
                return new List<T>() { obj };
            }

            IList<T> models = GetModels<T>(baseobj, objtype, objbuffer.Length, needsolitfile, obj, single);
            return models;
        }

        /// <summary>
        /// 获取此对象 是否  有集合
        /// </summary>
        /// <param name="objtype"></param>
        /// <returns></returns>
        private static Dictionary<string, Type> GetListTypes(PropertyInfo[] proarry)
        {
            Dictionary<string, Type> typedic = new Dictionary<string, Type>();
            foreach (PropertyInfo item in proarry)
            {
                Type itemp = item.PropertyType;
                if (itemp.IsGenericType && (typeof(IList<>).GUID == itemp.GUID || typeof(List<>).GUID == itemp.GUID))
                {
                    typedic.Add(item.Name, itemp);
                }
                //else if (NBinaryFormatter.GetSize(itemp) == 0 && itemp != typeof(string))
                //{
                //    //Dictionary<string, Type> temptypes = GetListTypes(itemp);
                //    //if (temptypes != null && temptypes.Any())
                //    //{
                //    //    foreach (var type in temptypes)
                //    //    {
                //    //        typedic.Add(type.Key, type.Value);
                //    //    }
                //    //}

                //    typedic.Add(item.Name, itemp);
                //}
            }

            return typedic;
        }



        /// <summary>
        /// 获取基础类型的基础对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        private static T GetBaseModel<T>(T model, Type modeltype, PropertyInfo[] proarry) where T : new()
        {
            object tempmodel = Activator.CreateInstance(modeltype);

            foreach (var item in proarry)
            {
                if (BinaryFormatter.SizeBy(item.PropertyType) == 0 && item.PropertyType != typeof(string))
                {
                    continue;
                }

                object obj = item.GetValue(model, null);

                PropertyInfo newpro = modeltype.GetProperty(item.Name);

                newpro.SetValue(tempmodel, obj, null);
            }

            return (T)tempmodel;
        }


        private static IList<T> GetModels<T>(T basemodel, Type modeltype, int basemodelsize, Dictionary<string, Type> dic, T oldmodel, bool single) where T : class, new()
        {
            List<T> models = new List<T>();

            bool isnext = false;
            foreach (var item in dic)
            {
                PropertyInfo oldpinfo = modeltype.GetProperty(item.Key);
                IList oldpinfovalue = oldpinfo.GetValue(oldmodel, null) as IList;

                if (oldpinfo == null || oldpinfovalue.Count == 0)
                {
                    continue;
                }

                bool isneedwhile = true;

                int startindex = 0;
                do
                {
                    int thiswhilesize = basemodelsize;
                    var tempmodel = isnext ? models.Last() : basemodel.Copy<T>(modeltype);
                    Type templisttype = typeof(List<>).MakeGenericType(GetArrayElement(oldpinfo.PropertyType));

                    var templistvalue = Activator.CreateInstance(templisttype);

                    for (int i = startindex; i < oldpinfovalue.Count; i = startindex)
                    {
                        byte[] tempbuff = (oldpinfovalue[i] as ISerializable).Serialize();
                        //if (tempbuff.Length > 1400)
                        //{
                        //    throw new Exception("一个对象不能超过1400字节");
                        //}
                        if (thiswhilesize + tempbuff.Length > NBufferSplit.MAX_BUFFER_BLOCK_SIZE)
                        {
                            oldpinfo.SetValue(tempmodel, templistvalue, null);
                            if (models.Any() && models[models.Count - 1] == tempmodel)
                            {
                                models[models.Count - 1] = tempmodel;
                            }
                            else
                            {
                                models.Add(tempmodel);
                            }

                            isnext = false;
                            break;
                        }

                        templisttype.GetMethod("Add").Invoke(templistvalue, new object[] { oldpinfovalue[i] });

                        if (startindex == oldpinfovalue.Count - 1)
                        {
                            if (thiswhilesize + tempbuff.Length < NBufferSplit.MAX_BUFFER_BLOCK_SIZE)
                            {
                                isnext = true;
                            }

                            oldpinfo.SetValue(tempmodel, templistvalue, null);
                            if (models.Any() && models[models.Count - 1] == tempmodel)
                            {
                                models[models.Count - 1] = tempmodel;
                            }
                            else
                            {
                                models.Add(tempmodel);
                            }

                            isneedwhile = false;
                            break;
                        }

                        thiswhilesize += tempbuff.Length;
                        startindex++;
                    }

                } while (isneedwhile);
            }

            return models;
        }

        private static Type GetArrayElement(Type array)
        {
            if (array.IsArray)
            {
                return array.GetElementType();
            }
            if (array.IsGenericType && (typeof(IList<>).GUID == array.GUID || typeof(List<>).GUID == array.GUID))
            {
                Type[] args = array.GetGenericArguments();
                return args[0];
            }
            return null;
        }
    }
}
