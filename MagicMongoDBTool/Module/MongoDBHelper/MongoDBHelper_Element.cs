﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Bson;


namespace MagicMongoDBTool.Module
{
    public static partial class MongoDBHelper
    {

        public static Object _ClipElement = null;
        public static Boolean _IsElementClip = true;
        /// <summary>
        /// Can Paste As Value
        /// </summary>
        public static Boolean CanPasteAsValue
        {
            get { return (_ClipElement != null && !_IsElementClip); }
        }
        /// <summary>
        /// Can Paste As Element
        /// </summary>
        public static Boolean CanPasteAsElement
        {
            get { return (_ClipElement != null && _IsElementClip); }
        }

        /// <summary>
        /// Paste
        /// </summary>
        /// <param name="ElementPath"></param>
        public static void PasteElement(String ElementPath)
        {
            BsonDocument BaseDoc = SystemManager.GetCurrentDocument();
            BsonValue t = GetLastParentDocument(BaseDoc, ElementPath, true);
            if (t.IsBsonDocument)
            {
                t.AsBsonDocument.InsertAt(t.AsBsonDocument.ElementCount, (BsonElement)_ClipElement);
            }
            SystemManager.GetCurrentCollection().Save(BaseDoc);
        }
        public static void PasteValue(String ElementPath)
        {
            BsonDocument BaseDoc = SystemManager.GetCurrentDocument();
            BsonValue t = GetLastParentDocument(BaseDoc, ElementPath, true);
            if (t.IsBsonArray)
            {
                t.AsBsonArray.Insert(t.AsBsonArray.Count, (BsonValue)_ClipElement);
            }
            SystemManager.GetCurrentCollection().Save(BaseDoc);
        }
        /// <summary>
        /// Cut Element
        /// </summary>
        /// <param name="ElementPath"></param>
        public static void CopyElement(BsonElement El)
        {
            _ClipElement = El;
            _IsElementClip = true;
        }
        /// <summary>
        /// Cut Array Value
        /// </summary>
        /// <param name="ElementPath"></param>
        public static void CopyValue(BsonValue Val)
        {
            _ClipElement = Val;
            _IsElementClip = false;
        }
        /// <summary>
        /// Cut Element
        /// </summary>
        /// <param name="ElementPath"></param>
        public static void CutElement(String ElementPath, BsonElement El)
        {
            _ClipElement = El;
            _IsElementClip = true;
            DropElement(ElementPath, El);
        }
        /// <summary>
        /// Cut Array Value
        /// </summary>
        /// <param name="ElementPath"></param>
        public static void CutValue(String ElementPath, int ValueIndex, BsonValue Val)
        {
            _ClipElement = Val;
            _IsElementClip = false;
            DropArrayValue(ElementPath, ValueIndex);
        }
        /// <summary>
        /// Add Element
        /// </summary>
        /// <param name="BaseDoc"></param>
        /// <param name="AddElement"></param>
        public static void AddElement(String ElementPath, BsonElement AddElement)
        {
            BsonDocument BaseDoc = SystemManager.GetCurrentDocument();
            BsonValue t = GetLastParentDocument(BaseDoc, ElementPath, true);
            if (t.IsBsonDocument)
            {
                t.AsBsonDocument.InsertAt(t.AsBsonDocument.ElementCount, AddElement);
            }
            SystemManager.GetCurrentCollection().Save(BaseDoc);
        }
        /// <summary>
        /// Add Value
        /// </summary>
        /// <param name="ElementPath"></param>
        /// <param name="AddValue"></param>
        public static void AddArrayValue(String ElementPath, BsonValue AddValue)
        {
            BsonDocument BaseDoc = SystemManager.GetCurrentDocument();
            BsonValue t = GetLastParentDocument(BaseDoc, ElementPath, true);
            if (t.IsBsonArray)
            {
                t.AsBsonArray.Insert(t.AsBsonArray.Count, AddValue);
            }
            SystemManager.GetCurrentCollection().Save(BaseDoc);
        }

        /// <summary>
        /// Drop Element
        /// </summary>
        /// <param name="BaseDoc"></param>
        /// <param name="ElementPath"></param>
        public static void DropElement(String ElementPath, BsonElement El)
        {
            BsonDocument BaseDoc = SystemManager.GetCurrentDocument();
            BsonValue t = GetLastParentDocument(BaseDoc, ElementPath);
            if (t.IsBsonDocument)
            {
                t.AsBsonDocument.Remove(El.Name);
            }
            SystemManager.GetCurrentCollection().Save(BaseDoc);
        }
        /// <summary>
        /// Drop A Value of Array
        /// </summary>
        /// <param name="BaseDoc"></param>
        /// <param name="ElementPath"></param>
        public static void DropArrayValue(String ElementPath, int ValueIndex)
        {
            BsonDocument BaseDoc = SystemManager.GetCurrentDocument();
            BsonValue t = GetLastParentDocument(BaseDoc, ElementPath);
            if (t.IsBsonArray)
            {
                t.AsBsonArray.RemoveAt(ValueIndex);
            }
            SystemManager.GetCurrentCollection().Save(BaseDoc);
        }
        /// <summary>
        /// Modify Element
        /// </summary>
        /// <param name="ElementPath"></param>
        /// <param name="NewValue"></param>
        /// <param name="ValueIndex"></param>
        /// <param name="El"></param>
        public static void ModifyElement(String ElementPath, BsonValue NewValue, BsonElement El)
        {
            BsonDocument BaseDoc = SystemManager.GetCurrentDocument();
            BsonValue t = GetLastParentDocument(BaseDoc, ElementPath);
            if (t.IsBsonDocument)
            {
                t.AsBsonDocument.GetElement(El.Name).Value = NewValue;
            }
            SystemManager.GetCurrentCollection().Save(BaseDoc);
        }
        /// <summary>
        /// Modify A Value of Array
        /// </summary>
        /// <param name="ElementPath"></param>
        /// <param name="NewValue"></param>
        /// <param name="ValueIndex"></param>
        /// <param name="El"></param>
        public static void ModifyArrayValue(String ElementPath, BsonValue NewValue, int ValueIndex)
        {
            BsonDocument BaseDoc = SystemManager.GetCurrentDocument();
            BsonValue t = GetLastParentDocument(BaseDoc, ElementPath);
            if (t.IsBsonArray)
            {
                t.AsBsonArray[ValueIndex] = NewValue;
            }
            SystemManager.GetCurrentCollection().Save(BaseDoc);
        }


        /// <summary>
        /// Locate the Operation Place
        /// </summary>
        /// <param name="BaseDoc"></param>
        /// <param name="ElementPath"></param>
        /// <param name="IsGetLast">T:GetOperationPlace F:GetOperationPlace Parent</param>
        /// <returns></returns>
        public static BsonValue GetLastParentDocument(BsonDocument BaseDoc, String ElementPath, Boolean IsGetLast = false)
        {
            BsonValue Current = BaseDoc;
            //JpCnWord[1]\Translations[ARRAY]\Translations[1]\Sentences[ARRAY]\Sentences[1]\Japanese:"ああいう文章はなかなか書けない"
            //1.将路径按照\分开
            String[] strPath = ElementPath.Split(@"\".ToCharArray());
            //JpCnWord[1]                                    First
            //Translations[ARRAY]
            //Translations[1]
            //Sentences[ARRAY]
            //Sentences[1]
            //Japanese:"ああいう文章はなかなか書けない"        Last
            int DeepLv;
            if (IsGetLast)
            {
                DeepLv = strPath.Length;
            }
            else
            {
                DeepLv = strPath.Length - 1;
            }
            for (int i = 1; i < DeepLv; i++)
            {
                String strTag = strPath[i];
                Boolean IsArray = false;
                if (strTag.EndsWith(Array_Mark))
                {
                    //去除[Array]后缀
                    strTag = strTag.Substring(0, strTag.Length - Array_Mark.Length);
                    IsArray = true;
                }
                if (IsArray)
                {
                    //这里的Array是指一个列表的上层节点，在BSON里面没有相应的对象，只是个逻辑概念
                    Current = Current.AsBsonDocument.GetValue(strTag).AsBsonArray;
                }
                else
                {
                    if (Current.IsBsonArray)
                    {
                        //当前的如果是数组，获得当前下标。
                        int Index = Convert.ToInt16(strTag.Substring(strTag.IndexOf("[".ToString()) + 1, strTag.Length - strTag.IndexOf("[".ToString()) - 2));
                        Current = Current.AsBsonArray[Index - 1];
                    }
                    else
                    {
                        if (Current.IsBsonDocument)
                        {
                            //如果当前还是一个文档的话
                            Current = Current.AsBsonDocument.GetValue(strTag);
                        }
                        else
                        {
                            //不应该会走到这个分支
                            return null;
                        }
                    }
                }
            }
            return Current;
        }

    }
}