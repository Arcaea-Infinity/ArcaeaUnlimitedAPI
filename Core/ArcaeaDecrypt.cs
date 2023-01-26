using System.Text;

namespace ArcaeaUnlimitedAPI.Core;

internal sealed unsafe class ArcaeaDecrypt
{
    private const int CertLen = 1638;

    private const int EndpointMaxLen = 100;

    private static readonly byte[] CertBytes = { 0x43, 0xD1, 0x4B, 0x08, 0x8C, 0xBB, 0xAE, 0x0E, 0xDC, 0x76, 0xF9, 0x21, 0xDE, 0x23, 0x6E, 0x4A },
                                   EndpointBytes = { 0x1B, 0x27, 0x39, 0x1A, 0xFD, 0x80, 0x82, 0x11 },
                                   SaltBytes = { 0x20, 0x1C, 0x20, 0x6E };

    private byte[] _lib = null!;

    internal void ReadLib(byte[] lib) => _lib = lib;

    internal byte[] GetCert()
    {
        var pos = IndexOf(CertBytes).FirstOrDefault();
        return DecryptBytes(_lib[pos.. (pos + CertLen)]);
    }

    internal string GetApiEntry()
    {
        var pos = IndexOf(EndpointBytes).FirstOrDefault();
        var @in = _lib[pos..(pos + EndpointMaxLen)];
        var bytes = DecryptBytes(@in);
        var str = Encoding.ASCII.GetString(bytes);
        return str[(str.LastIndexOf("lowiro.com/", StringComparison.Ordinal) + 11)..(str.IndexOf((char)0) - 1)];
    }

    internal byte[] GetSalt()
    {
        var ls = new List<byte[]>();

        foreach (var posEor in IndexOf(SaltBytes))
        {
            for (var pos = posEor - 40; pos < posEor + 40; pos += 4)
            {
                var insn = ParseInstruction(pos);
                if (insn == 0xD65F03C0 /* RET */) break;
                
                if ((insn & 0x9F000000) != 0x90000000) continue;
                
                var adrpImm = ((pos >> 12) + ((Slice(insn, 5, 19) << 2) | Slice(insn, 29, 2))) << 12;
                insn = ParseInstruction(pos += 4);
                if ((insn & 0x7FC00000) != 0x11000000) continue;
                
                var addImm = Slice(insn, 10, 12);
                if (Slice(insn, 0, 5) != Slice(insn, 5, 5)) continue;
                
                var saltAddr = (int)(adrpImm + addImm - 0x2000);
                var decrypted = DecryptBytes(_lib[saltAddr..(saltAddr + 32)]);
                if (ls.Any(i => i.SequenceEqual(decrypted))) continue;
                ls.Add(decrypted);
            }
        }

        if (ls.Any())
        {
            byte[]? result = null;

            foreach (var b in ls)
            {
                if (result is null)
                {
                    result = b;
                    continue;
                }

                for (var i = 0; i < b.Length; ++i) result[i] ^= b[i];
            }

            return result!;
        }

        return Array.Empty<byte>();
    }

    private static byte[] DecryptBytes(byte[] @in) => new DecryptContext().CfbDecrypt(@in);

    private static bool SequenceEqual(byte[] lib, byte[] eor, int offset = 0)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var i = 0; i < eor.Length; ++i)
        {
            if (lib[offset + i] != eor[i]) return false;
        }

        return true;
    }

    private IEnumerable<int> IndexOf(byte[] eor)
    {
        for (var pos = 0; pos < _lib.Length; pos += 4)
        {
            if (SequenceEqual(_lib, eor, pos)) yield return pos;
        }
    }

    private uint ParseInstruction(int pos) => BitConverter.ToUInt32(_lib[pos..(pos + 4)]);

    private static long Slice(uint n, int offset, int size = 1) => (n >> offset) & ((1 << size) - 1);

    private class DecryptContext
    {
        private const byte Blocksize = 0x10;

        private const uint MagicInit = 0x7aa1c11a;

        private static readonly uint[] S =
        {
            0xb09d1ca7, 0xc569477c, 0x1852c462, 0x93396ad4, 0xef60ea00, 0x5eb3faf3, 0xe351c122, 0xe063ec8e, 0xa819edf9, 0x3b283cf4, 0x0cb59f7d,
            0x606bdb1c, 0x86b14669, 0xaf193d43, 0xb2c346bf, 0xe5742815, 0xe4c30705, 0x2ff7d950, 0x4ca039cd, 0x8a35d87e, 0xc9205f62, 0x4a37169f,
            0xcfa3d3ac, 0x2a9db64b, 0x59f61fba, 0x9f218566, 0xd0dd8ae5, 0x16bdc6af, 0x41da582f, 0x3683143c, 0x491ab4a2, 0x243e2dc1, 0x392f47fe,
            0xedb59ddc, 0x3318b86b, 0x900e11fd, 0x209f07da, 0xc771b6fe, 0x55143762, 0x5cf9e3e3, 0x29e103cf, 0x8da0ef8b, 0x434881bb, 0xfb282a0f,
            0xd66c75fa, 0xc4d522ba, 0x000ec7f1, 0x6f926f16, 0xb5fedad2, 0xa6fef87f, 0x4a10b783, 0x9a9269ff, 0xd512b130, 0x45388187, 0xbfdf2a51,
            0x3dfc8c36, 0x57a21a0a, 0x84c2f28f, 0xccfcc693, 0xdd78f17d, 0xd85c6634, 0x383c87c3, 0x47a9a2d5, 0x8ed7458f, 0x6a181b37, 0xe40c9dce,
            0x2a915ae0, 0x5f4712e6, 0x96400d84, 0xc5056a21, 0xd48584f9, 0x4fcc1038, 0xb598f607, 0xee638623, 0x435c99b6, 0xb220ace0, 0x8fe2bb4e,
            0xcfd97ca8, 0x72cbfa44, 0xe49697ed, 0x163a05a4, 0xfb994d10, 0x9b364b16, 0xc59f74a8, 0x789a680a, 0x0715c258, 0xfa1749c7, 0xa4f5e379,
            0x113720ae, 0x120c404b, 0xc93a9be2, 0x1723acb5, 0x295bc781, 0x45c9f550, 0x95f69575, 0xdc4c8e46, 0x462c5a49, 0xa79f661e, 0xb18afe15,
            0xd7c76cec, 0x440d6eed, 0x7062ed16, 0x10572999, 0x1de817e1, 0x3e3cb54d, 0x7ab7bc1c, 0xed853343, 0x2eefad31, 0x1281b833, 0xe6627df7,
            0xe66b2b88, 0x0ac31d4e, 0xf4ec5056, 0x9e53ac46, 0xd359128d, 0xa248c135, 0xd758ef1c, 0x98ffa726, 0xbe509df9, 0x442d9513, 0x8a1362fd,
            0x81dc59ed, 0xbf268b70, 0x8c98b606, 0xb28ecad3, 0x298465a9, 0x274d1d91, 0x71f034a4, 0xd14ae97c, 0x7fbcc1af, 0x0feae001, 0xfe2943db,
            0xdefb757e, 0xaaf7fbb9, 0x58252b2f, 0xd7e555a4, 0xbc4d5a4c, 0x48881c9c, 0x9ee0c2c0, 0x4c80e000, 0x4128561f, 0x59eebc82, 0xfaed1e4b,
            0x84bf8822, 0x0ae6e774, 0x498d7959, 0xb1fb7373, 0x9f1a9275, 0x0ab59981, 0x40229487, 0x0af6c569, 0x2e647b3c, 0x43409f49, 0xfe46f697,
            0x550b5cda, 0x41eb4038, 0xd2bb8856, 0xd217ddd0, 0xbc9f096d, 0x2843c053, 0xc3a314c9, 0x3a394631, 0xd0ff0c0b, 0x9c5c376a, 0x28c18af2,
            0xbb74c83b, 0x9e0b0bd3, 0x33d26080, 0xebc25e6f, 0x3877fcc7, 0xed90f57f, 0xdc37fa01, 0x995a33f6, 0xc3e202d1, 0xbed70d53, 0xfc8cda16,
            0x97500b88, 0xe906afbe, 0xbdedb238, 0xd32856d5, 0x766fef6f, 0x628ee763, 0x58ba73fd, 0x9f8e3986, 0x6738d10d, 0x516316d8, 0xd6913740,
            0x03d23ef0, 0x49470b3c, 0x9013e92b, 0xf60ac5cc, 0x090163b5, 0x7e639935, 0xd6f942f7, 0xf24dba56, 0x4c7fe480, 0xc3c77822, 0x87254d97,
            0xf7a9d29a, 0x24937b58, 0xf2f8fb9d, 0xa330cea4, 0xa852f0fa, 0xa6a831fa, 0x20e5ae64, 0x5243568c, 0xd3e17087, 0x3e56a04a, 0xcf54110a,
            0x2d25b38a, 0x7d565d07, 0xb278b538, 0xb26671ad, 0xa9b66c5b, 0xf265f493, 0x657550cd, 0x27f07651, 0x1906a88b, 0x47ce4ecc, 0xaade90ed,
            0x447a9aa1, 0x48553310, 0xddbd25c8, 0x24a67b2b, 0xfb9eb1f6, 0x85a0a9d8, 0xe0e40011, 0x4efdcd01, 0x65845966, 0xb9740e16, 0xd12e4402,
            0x318ba27e, 0xc9db749a, 0xcc99acec, 0x644caaab, 0x371e20f5, 0x0c3e8e7a, 0x580e6fa1, 0x46e92102, 0x506b1898, 0xbd8ffa3d, 0x8e84c2b7,
            0x48214ba3, 0xf3f66756, 0xb02e266c, 0xc3173f15, 0x500dc4de, 0x0751a5ba, 0x56c7b66f, 0x42caadb8, 0xd3e22951, 0xe93f9288, 0xead1011f,
            0xb787d720, 0xc7aecfec, 0xaa693960
        };

        private readonly uint[] _rk = new uint[17];
        private uint _magic;
        private uint _mixCount;
        private uint _mixStep;

        public DecryptContext()
        {
            Setup();
        }

        private uint MixCtx()
        {
            unchecked
            {
                _mixCount++;

                var newVal = MixS(MixTriple(_magic, _rk[0], _rk[15]), _rk[4]);
                for (var i = 0; i < 0x10; i++) _rk[i] = _rk[i + 1];

                _rk[16] = newVal;
                var value = (_rk[16] + _rk[0]) ^ (_rk[13] + _rk[1]) ^ (_rk[6] + _magic);

                if (_mixCount < 0x10001) return value;

                // Note: Untested, only used for very long string
                _mixCount -= 0x10001;
                _mixStep += 0x10001;
                _rk[2] += _mixStep;
                _magic = value;
                return MixCtx();
            }
        }

        private void Setup()
        {
            unchecked
            {
                _rk[0] = 1;
                _rk[1] = 1;
                for (var i = 0; i < 0xf; i++) _rk[i + 2] = _rk[i + 1] + _rk[i];

                _magic = MagicInit;

                _mixCount = 0;
                _mixStep = 0;
                for (var i = 0; i < 0x14; i++) _rk[4] = MixCtx() ^ _rk[4];

                _rk[15] += 0x50;
                for (var i = 0; i < 17; i++) _rk[4] = MixCtx() ^ _rk[4];

                _magic = MixCtx();
                _mixCount = 0;
                _mixStep = 0;
            }
        }

        private bool Decrypt(byte* src, byte* dest)
        {
            unchecked
            {
                for (var i = 0; i < 16; i++) dest[i] = src[i];

                for (var i = 0; i < 4; i++)
                {
                    var value = MixCtx();
                    for (var j = 0; j < 4; j++) dest[4 * i + j] ^= (byte)(value >> (8 * j));
                }
            }

            return true;
        }

        public byte[] CfbDecrypt(byte[] input)
        {
            unchecked
            {
                var len = input.Length;
                var output = new byte[len];

                fixed (byte* vi = new byte[Blocksize], @in = input, bk = new byte[32])
                {
                    var p = vi;
                    for (var i = 0; i < len; i += Blocksize)
                    {
                        if (!Decrypt(p, bk)) return output;

                        var chunksz = Math.Min(Blocksize, len - i);
                        for (var j = 0; j < chunksz; j++) output[i + j] = (byte)(bk[j] ^ input[i + j]);

                        p = @in + i;
                    }
                }

                return output;
            }
        }

        private static uint Ror(uint w, int i)
        {
            unchecked
            {
                return (w >> i) | (w << (32 - i));
            }
        }

        private static uint MixS(uint v, uint offset) => v ^ S[v >> 24] ^ offset;

        private static uint MixTriple(uint r0X0, uint r0Xd, uint r0X17)
        {
            unchecked
            {
                return r0X0 + Ror(r0Xd, 0xd) + Ror(r0X17, 0x17);
            }
        }
    }
}
