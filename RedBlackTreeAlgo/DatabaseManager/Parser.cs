using System;
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
    }
}
