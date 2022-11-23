using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedBlackTreeAlgo.DatabaseManager
{
    public class Parser
    {
        private static char IntCh = 'I';
        private static char DoubleCh = 'D';
        private static char CharCh = 'C';

        private static string IntS = "int";
        private static string DoubleS = "double";
        private static string CharS = "char";

        public static byte[] CreateMetadataForDB(string text, out int totalSize)
        {
            //size-of-one-record
            // size-letter-strLenght-str
            //column_name type
            const char StatementDelim = ',';
            const char wordsDelim = ' ';
            List<byte> metadata = new List<byte>();
            byte[] byteArr;
            text = text.ToLower();
            totalSize = 0;
            string[] lines = text.Split(StatementDelim);    //divide into statements
            for (int i=0; i<lines.Length; i++)
                lines[i] = lines[i].Trim();
            for(int i=0; i<lines.Length; i++)
            {
                string[] sublines = lines[i].Split(wordsDelim);
                if (sublines[^1]== IntS)    //if type is INT
                {
                    byteArr = BitConverter.GetBytes(sizeof(int));
                    foreach (byte b in byteArr)
                        metadata.Add(b);
                    byteArr = BitConverter.GetBytes(IntCh);
                    foreach (byte b in byteArr)
                        metadata.Add(b);
                    totalSize += sizeof(int);
                }
                else if(sublines[^1] == DoubleS)    //if type is DOUBLE
                {
                    byteArr = BitConverter.GetBytes(sizeof(double));
                    foreach (byte b in byteArr)
                        metadata.Add(b);
                    byteArr = BitConverter.GetBytes(DoubleCh);
                    foreach (byte b in byteArr)
                        metadata.Add(b);
                    totalSize += sizeof(double);
                }
                else if(sublines[^1].Contains(CharS))   //if type is STRING
                {
                    string valueType = sublines[^1];
                    int pos = valueType.IndexOf('(');
                    pos++;
                    string lenght = "";
                    while (valueType[pos]!=')' && pos < valueType.Length)
                    {
                        lenght += valueType[pos];
                        pos++;
                    }
                    int lenghtOfString = Convert.ToInt32(lenght);
                    totalSize += lenghtOfString;
                    byteArr = BitConverter.GetBytes(lenghtOfString);
                    foreach (byte b in byteArr)
                        metadata.Add(b);
                    byteArr = BitConverter.GetBytes(CharCh);
                    foreach (byte b in byteArr)
                        metadata.Add(b);
                }

                byteArr = BitConverter.GetBytes(sublines[0].Length);
                foreach (byte b in byteArr)
                    metadata.Add(b);
                byteArr = Encoding.ASCII.GetBytes(sublines[0]);
                foreach (byte b in byteArr)
                    metadata.Add(b);
            }
            byte[] byteTotalSize = BitConverter.GetBytes(totalSize);
            byte[] finalMetadata = new byte[metadata.Count + byteTotalSize.Length];
            byteTotalSize.CopyTo(finalMetadata, 0);
            metadata.ToArray().CopyTo(finalMetadata, byteTotalSize.Length);
            return finalMetadata;
        }
        public static byte[] DataToByte(List<(int typeSize, char t, string cName)> colmns, string data, int recordDataSize)
        {            
            byte[] dataBytes = new byte[recordDataSize];
            int pos = 0;
            int i = 0;
            string[] strings = data.Split(',');
            for (int j = 0; j < strings.Length; j++)
                strings[j] = strings[j].Trim();
            while (i < colmns.Count)
            {
                if (colmns[i].t == IntCh)
                {
                    int dt = Convert.ToInt32(strings[i]);
                    Array.Copy(BitConverter.GetBytes(dt), 0, dataBytes, pos, sizeof(int));
                    pos += sizeof(int);
                }
                else if (colmns[i].t == DoubleCh)
                {
                    strings[i] = strings[i].Replace(".", ",");
                    double dt = Convert.ToDouble(strings[i]);
                    Array.Copy(BitConverter.GetBytes(dt), 0, dataBytes, pos, sizeof(double));
                    pos += sizeof(double);
                }
                else
                {
                    Array.Copy(Encoding.ASCII.GetBytes(strings[i]), 0, dataBytes, pos, strings[i].Length);
                    pos += colmns[i].typeSize;               
                }
                i++;
            }
            return dataBytes;
        }

        public static List<(int typeSize, char t, string cName)> MetadataToData(byte[] metadata)
        {
            (int typeSize, char type, string cName) colmn;
            List<(int typeSize, char t, string cName)> colmns = new List<(int typeSize, char t, string cName)>();
            int pos = 0;
            int recordDataSize = BitConverter.ToInt32(metadata, pos);
            pos += sizeof(int);
            while (pos < metadata.Length)
            {
                colmn.typeSize = BitConverter.ToInt32(metadata, pos);
                colmn.type = BitConverter.ToChar(metadata, pos+=sizeof(int));
                int nameLengthBytes = BitConverter.ToInt32(metadata, pos += sizeof(char));
                byte[] arr = new byte[nameLengthBytes];
                Array.Copy(metadata, pos += sizeof(int), arr, 0, nameLengthBytes);
                colmn.cName = System.Text.Encoding.UTF8.GetString(arr);
                pos += nameLengthBytes;
                colmns.Add(colmn);
            }
            return colmns;
        }
        public static string BytesToData(List<(int typeSize, char t, string cName)> colmns, byte[] data)
        {            
            string dataString = "";           
            int i = 0;
            int pos = 0;
            while (i < colmns.Count)
            {
                dataString += colmns[i].cName + ": ";
                if(colmns[i].t == IntCh)
                {
                    int dt = BitConverter.ToInt32(data, pos);
                    pos += sizeof(int);
                    dataString += Convert.ToString(dt);
                }
                else if (colmns[i].t == DoubleCh)
                {
                    double dt = BitConverter.ToDouble(data, pos);
                    pos += sizeof(double);
                    dataString += Convert.ToString(dt);
                }
                else
                {
                    string dt = Encoding.ASCII.GetString(data, pos, colmns[i].typeSize);
                    dt = dt.Replace("\0", "");
                    pos += colmns[i].typeSize;
                    dataString += dt;
                }
                dataString += "; \n";
                i++;
            }
            return dataString;
        }
    }
}
