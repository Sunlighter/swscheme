/*
    This file is part of Sunlit World Scheme
    http://swscheme.codeplex.com/
    Copyright (c) 2010 by Edward Kiser (edkiser@gmail.com)

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
*/

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using BigMath;

namespace ExprObjModel.Procedures
{
    [Flags]
    public enum AsymmetricKeyFlags : byte
    {
        None = 0,
        Encrypt = 1,
        Verify = 2,
        Decrypt = 4,
        Sign = 8
    }

    [SchemeIsAFunction("akey?")]
    public class AsymmetricKey
    {
        public byte[] encrypt;
        public byte[] decrypt;
        public byte[] verify;
        public byte[] sign;
    }

    [SchemeIsAFunction("ckey?")]
    public class ConventionalKey
    {
        public byte[] key;
        public byte[] iv;
    }

    public static partial class ProxyDiscovery
    {
        public static AsymmetricKey MakeAsymmetricKey(bool encrypt, bool sign)
        {
            AsymmetricKey a = new AsymmetricKey();

            if (encrypt)
            {
                CspParameters cp = new CspParameters(1);
                cp.Flags = CspProviderFlags.UseArchivableKey;
                cp.KeyNumber = (int)KeyNumber.Exchange;

                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048, cp))
                {
                    a.encrypt = rsa.ExportCspBlob(false);
                    a.decrypt = rsa.ExportCspBlob(true);
                }
            }

            if (sign)
            {
                CspParameters cp2 = new CspParameters(13);
                cp2.Flags = CspProviderFlags.UseArchivableKey;
                cp2.KeyNumber = (int)KeyNumber.Signature;

                using (DSACryptoServiceProvider dsa = new DSACryptoServiceProvider(1024, cp2))
                {
                    a.verify = dsa.ExportCspBlob(false);
                    a.sign = dsa.ExportCspBlob(true);
                }
            }

            return a;
        }

        [SchemeFunction("begin-make-akey")]
        public static AsyncID BeginMakeAsymmetricKey(IGlobalState gs, bool encrypt, bool sign)
        {
            Func<bool, bool, AsymmetricKey> f = new Func<bool,bool,AsymmetricKey>(MakeAsymmetricKey);

            IAsyncResult iar = f.BeginInvoke(encrypt, sign, null, null);

            CompletionProc c = delegate(IGlobalState gs2, IAsyncResult iar2)
            {
                AsymmetricKey a = f.EndInvoke(iar2);
                return a;
            };

            return gs.RegisterAsync(iar, c, "begin-make-akey");
        }

        [SchemeFunction("akey-can-encrypt?")]
        public static bool CanEncrypt(AsymmetricKey a)
        {
            return a.encrypt != null;
        }

        [SchemeFunction("akey-can-decrypt?")]
        public static bool CanDecrypt(AsymmetricKey a)
        {
            return a.decrypt != null;
        }

        [SchemeFunction("akey-can-verify?")]
        public static bool CanVerify(AsymmetricKey a)
        {
            return a.verify != null;
        }

        [SchemeFunction("akey-can-sign?")]
        public static bool CanSign(AsymmetricKey a)
        {
            return a.sign != null;
        }

        [SchemeFunction("akey-get-public")]
        public static AsymmetricKey ExtractPublicPart(AsymmetricKey a)
        {
            AsymmetricKey b = new AsymmetricKey();
            b.encrypt = a.encrypt;
            b.verify = a.verify;
            return b;
        }

        [SchemeFunction("akey-encrypt")]
        public static SchemeByteArray AsymmetricEncrypt(AsymmetricKey a, SchemeByteArray data)
        {
            if (a.encrypt == null) throw new SchemeRuntimeException("Key does not allow encryption");

            CspParameters cp = new CspParameters(1);
            cp.Flags = CspProviderFlags.NoFlags;
            cp.KeyNumber = (int)KeyNumber.Exchange;

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(cp))
            {
                rsa.ImportCspBlob(a.encrypt);
                byte[] data2 = rsa.Encrypt(data.Bytes, false);
                return new SchemeByteArray(data2, DigitOrder.LBLA);
            }
        }

        [SchemeFunction("akey-decrypt")]
        public static SchemeByteArray AsymmetricDecrypt(AsymmetricKey a, SchemeByteArray data)
        {
            if (a.decrypt == null) throw new SchemeRuntimeException("Key does not allow decryption");

            CspParameters cp = new CspParameters(1);
            cp.Flags = CspProviderFlags.NoFlags;
            cp.KeyNumber = (int)KeyNumber.Exchange;

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(cp))
            {
                rsa.ImportCspBlob(a.decrypt);
                byte[] data2 = rsa.Decrypt(data.Bytes, false);
                return new SchemeByteArray(data2, DigitOrder.LBLA);
            }
        }

        [SchemeFunction("akey-verify")]
        public static bool AsymmetricVerify(AsymmetricKey a, SchemeByteArray data, SchemeByteArray signature)
        {
            if (a.verify == null) throw new SchemeRuntimeException("Key does not allow verification");

            CspParameters cp = new CspParameters(13);
            cp.Flags = CspProviderFlags.NoFlags;
            cp.KeyNumber = (int)KeyNumber.Signature;

            using (DSACryptoServiceProvider dsa = new DSACryptoServiceProvider(cp))
            {
                dsa.ImportCspBlob(a.verify);
                return dsa.VerifyData(data.Bytes, signature.Bytes);
            }
        }

        [SchemeFunction("akey-sign")]
        public static SchemeByteArray AsymmetricSign(AsymmetricKey a, SchemeByteArray data)
        {
            if (a.sign == null) throw new SchemeRuntimeException("Key does not allow signing");

            CspParameters cp = new CspParameters(13);
            cp.Flags = CspProviderFlags.NoFlags;
            cp.KeyNumber = (int)KeyNumber.Signature;

            using (DSACryptoServiceProvider dsa = new DSACryptoServiceProvider(cp))
            {
                dsa.ImportCspBlob(a.sign);
                byte[] data2 = dsa.SignData(data.Bytes);
                return new SchemeByteArray(data2, DigitOrder.LBLA);
            }
        }

        [SchemeFunction("akey->bytes")]
        public static SchemeByteArray AsymmetricKeyToBytes(AsymmetricKey a)
        {
            byte q = 0;
            if (a.encrypt != null) q |= (byte)(AsymmetricKeyFlags.Encrypt);
            if (a.decrypt != null) q |= (byte)(AsymmetricKeyFlags.Decrypt);
            if (a.verify != null) q |= (byte)(AsymmetricKeyFlags.Verify);
            if (a.sign != null) q |= (byte)(AsymmetricKeyFlags.Sign);

            using (MemoryStream m = new MemoryStream())
            {
                m.WriteByte(q);
                if (a.encrypt != null) m.WriteByteArray(a.encrypt);
                if (a.decrypt != null) m.WriteByteArray(a.decrypt);
                if (a.verify != null) m.WriteByteArray(a.verify);
                if (a.sign != null) m.WriteByteArray(a.sign);

                return new SchemeByteArray(m.ToArray(), DigitOrder.LBLA);
            }
        }

        [SchemeFunction("bytes->akey")]
        public static AsymmetricKey BytesToAsymmetricKey(SchemeByteArray b)
        {
            using (MemoryStream m = new MemoryStream(b.Bytes))
            {
                AsymmetricKeyFlags q = (AsymmetricKeyFlags)(m.ReadByteOrDie());
                AsymmetricKey a = new AsymmetricKey();

                if (q.HasFlag(AsymmetricKeyFlags.Encrypt)) a.encrypt = m.ReadByteArray();
                if (q.HasFlag(AsymmetricKeyFlags.Decrypt)) a.decrypt = m.ReadByteArray();
                if (q.HasFlag(AsymmetricKeyFlags.Verify)) a.verify = m.ReadByteArray();
                if (q.HasFlag(AsymmetricKeyFlags.Sign)) a.sign = m.ReadByteArray();

                if (m.Position != m.Length) throw new FormatException("Bytes left after the end");

                return a;
            }
        }

        public static ConventionalKey MakeConventionalKey(int bits)
        {
            if (bits != 128 && bits != 192 && bits != 256) throw new SchemeRuntimeException("Number of bits must be 128, 192, or 256");

            using (RijndaelManaged rm = new RijndaelManaged())
            {
                rm.KeySize = bits;

                ConventionalKey key = new ConventionalKey();
                rm.GenerateKey();
                key.key = rm.Key;
                rm.GenerateIV();
                key.iv = rm.IV;

                return key;
            }
        }

        public static ConventionalKey MakeConventionalKey(int bits, byte[] bytes)
        {
            if (bits != 128 && bits != 192 && bits != 256) throw new SchemeRuntimeException("Number of bits must be 128, 192, or 256");

            using (SHA384Managed sha = new SHA384Managed())
            {
                byte[] xkey = new byte[bytes.Length + 2];
                Buffer.BlockCopy(bytes, 0, xkey, 1, bytes.Length);
                xkey[0] = (bits == 128) ? (byte)0x31 : (bits == 192) ? (byte)0x47 : (byte)0x1B;
                xkey[bytes.Length + 1] = (bits == 128) ? (byte)0xF2 : (bits == 192) ? (byte)0x9A : (byte)0x0C;
                byte[] hash = sha.ComputeHash(xkey);

                ConventionalKey key = new ConventionalKey();
                key.key = new byte[bits >> 3];
                Buffer.BlockCopy(hash, 0, key.key, 0, bits >> 3);
                key.iv = new byte[16];
                Buffer.BlockCopy(hash, 32, key.iv, 0, 16);

                return key;
            }
        }

        [SchemeFunction("ckey-encrypt")]
        public static SchemeByteArray ConventionalEncrypt(ConventionalKey c, SchemeByteArray data)
        {
            using (MemoryStream dest = new MemoryStream())
            {
                using (RijndaelManaged rm = new RijndaelManaged())
                {
                    rm.Mode = CipherMode.CFB;
                    rm.FeedbackSize = 32;
                    rm.Padding = PaddingMode.ISO10126;
                    rm.KeySize = 8 * c.key.Length;
                    rm.Key = c.key;
                    rm.IV = c.iv;

                    using (ICryptoTransform encryptor = rm.CreateEncryptor())
                    {
                        using (CryptoStream cs = new CryptoStream(dest, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(data.Bytes, 0, data.Bytes.Length);
                            cs.Close();
                        }
                    }
                }

                return new SchemeByteArray(dest.ToArray(), DigitOrder.LBLA);
            }
        }

        [SchemeFunction("ckey-decrypt")]
        public static SchemeByteArray ConventionalDecrypt(ConventionalKey c, SchemeByteArray data)
        {
            using (MemoryStream dest = new MemoryStream())
            {
                using (RijndaelManaged rm = new RijndaelManaged())
                {
                    rm.Mode = CipherMode.CFB;
                    rm.FeedbackSize = 32;
                    rm.Padding = PaddingMode.ISO10126;
                    rm.KeySize = 8 * c.key.Length;
                    rm.Key = c.key;
                    rm.IV = c.iv;

                    using (ICryptoTransform decryptor = rm.CreateDecryptor())
                    {
                        using (CryptoStream cs = new CryptoStream(dest, decryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(data.Bytes, 0, data.Bytes.Length);
                            cs.Close();
                        }
                    }
                }

                return new SchemeByteArray(dest.ToArray(), DigitOrder.LBLA);
            }
        }

        public static byte[] BytesForCrypto(object obj)
        {
            byte[] bytes;

            if (obj is SchemeString)
            {
                SchemeString str = (SchemeString)obj;
                bytes = System.Text.Encoding.UTF8.GetBytes(str.TheString);
            }
            else if (obj is SchemeByteArray)
            {
                bytes = ((SchemeByteArray)obj).Bytes;
            }
            else if (obj is BigInteger)
            {
                bytes = ((BigInteger)obj).GetByteArray(DigitOrder.LBLA);
            }
            else
            {
                throw new SchemeRuntimeException("make-ckey: data argument must be a string, byte array, or integer");
            }

            return bytes;
        }

        [SchemeFunction("ckey->bytes")]
        public static SchemeByteArray ConventionalKeyToBytes(ConventionalKey k)
        {
            using (MemoryStream m = new MemoryStream())
            {
                if (k.key.Length == 16)
                {
                    m.WriteByte(0x10);
                }
                else if (k.key.Length == 24)
                {
                    m.WriteByte(0x11);
                }
                else if (k.key.Length == 32)
                {
                    m.WriteByte(0x12);
                }
                else throw new SchemeRuntimeException("Unknown conventional key type");

                m.Write(k.key, 0, k.key.Length);
                m.Write(k.iv, 0, 16);

                return new SchemeByteArray(m.ToArray(), DigitOrder.LBLA);
            }
        }

        [SchemeFunction("bytes->ckey")]
        public static ConventionalKey BytesToConventionalKey(SchemeByteArray data)
        {
            using (MemoryStream m = new MemoryStream(data.Bytes))
            {
                ConventionalKey k = new ConventionalKey();
                byte b = m.ReadByteOrDie();
                if (b == 0x10)
                {
                    k.key = m.ReadFixedByteArray(16);
                }
                else if (b == 0x11)
                {
                    k.key = m.ReadFixedByteArray(24);
                }
                else if (b == 0x12)
                {
                    k.key = m.ReadFixedByteArray(32);
                }
                else
                {
                    throw new SchemeRuntimeException("Unknown conventional key format");
                }
                k.iv = m.ReadFixedByteArray(16);

                return k;
            }
        }

        [SchemeFunction("sha384")]
        public static SchemeByteArray SHA384(object data)
        {
            using (SHA384Managed sha = new SHA384Managed())
            {
                return new SchemeByteArray(sha.ComputeHash(BytesForCrypto(data)), DigitOrder.LBLA);
            }
        }
    }

    [SchemeSingleton("make-ckey")]
    public class MakeConventionalKeyProc : IProcedure
    {
        public MakeConventionalKeyProc() { }

        public int Arity { get { return 1; } }

        public bool More { get { return true; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            if (argList == null) throw new SchemeRuntimeException("make-ckey: insufficient arguments");
            if (!(argList.Head is BigInteger)) throw new SchemeRuntimeException("make-ckey: first argument must be an integer");
            BigInteger b = (BigInteger)(argList.Head);
            int bits = b.GetInt32Value(OverflowBehavior.ThrowException);
            argList = argList.Tail;

            if (argList == null)
            {
                return new RunnableReturn(k, ProxyDiscovery.MakeConventionalKey(bits));
            }
            else
            {
                byte[] bytes = ProxyDiscovery.BytesForCrypto(argList.Head);

                argList = argList.Tail;
                if (argList != null) throw new SchemeRuntimeException("make-ckey: too many arguments (must be 1 or 2)");

                return new RunnableReturn(k, ProxyDiscovery.MakeConventionalKey(bits, bytes));
            }
        }
    }
}
