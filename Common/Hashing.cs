using FileConverter.Hashes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Toolbox.Core.IO;

namespace NextLevelLibrary
{
    public class Hashing
    {
        private static Dictionary<uint, string> hashNames = new Dictionary<uint, string>();

        public static Dictionary<uint, string> HashNames
        {
            get
            {
                if (hashNames?.Count == 0)
                    LoadHashes();

                return hashNames;
            }
        }

        public static void LoadHashes()
        {
            LoadHashes(HashResources.FileNames);
            LoadHashes(HashResources.MaterialNames);
        }

        static void LoadHashes(string hashList)
        {
            foreach (string hashStr in hashList.Split('\n'))
            {
                string HashString = hashStr.TrimEnd();

                uint hash = StringToHash(HashString);
                uint lowerhash = StringToHash(HashString.ToLower());

                if (!hashNames.ContainsKey(hash))
                    hashNames.Add(hash, HashString);
                if (!hashNames.ContainsKey(lowerhash))
                    hashNames.Add(lowerhash, HashString.ToLower());

                string[] hashPaths = HashString.Split('/');
                for (int i = 0; i < hashPaths?.Length; i++)
                {
                    hash = StringToHash(hashPaths[i]);
                    if (!hashNames.ContainsKey(hash))
                        hashNames.Add(hash, hashPaths[i]);
                }
            }
        }

        static void LoadHash(string HashString)
        {
            uint hash = StringToHash(HashString);
            uint lowerhash = StringToHash(HashString.ToLower());

            if (!hashNames.ContainsKey(hash))
                hashNames.Add(hash, HashString);
            if (!hashNames.ContainsKey(lowerhash))
                hashNames.Add(lowerhash, HashString.ToLower());
        }

        //From (Works as tested comparing hashbin strings/hashes
        //https://gist.github.com/RoadrunnerWMC/f4253ef38c8f51869674a46ee73eaa9f

        /// <summary>
        /// Calculates a string to a hash value used by NLG files.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="caseSensative"></param>
        /// <returns></returns>
        public static uint StringToHash(string name, bool caseSensative = false)
        {
            byte[] data = Encoding.Default.GetBytes(name);

            int h = -1;
            for (int i = 0; i < data.Length; i++)
            {
                int c = (int)data[i];
                if (caseSensative && ((c - 65) & 0xFFFFFFFF) <= 0x19)
                    c |= 0x20;

                h = (int)((h * 33 + c) & 0xFFFFFFFF);
            }
            return (uint)h;
        }

        public static uint GetHashFromString(string str)
        {
            bool success = uint.TryParse(str, System.Globalization.NumberStyles.HexNumber, null, out uint hash);
            if (!success) return Hashing.StringToHash(str);

            return hash;
        }

        public static string CreateHashString(uint hash)
        {
            if (HashNames.ContainsKey(hash)) return HashNames[hash];
            return Reverse(hash.ToString("X"));
        }

        static string Reverse(string text)
        {
            int number = Convert.ToInt32(text, 16);
            byte[] bytes = BitConverter.GetBytes(number);
            string retval = "";
            foreach (byte b in bytes)
                retval += b.ToString("X2");
            return retval;
        }
    }
}
