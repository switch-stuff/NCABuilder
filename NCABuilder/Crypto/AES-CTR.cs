using System.Collections.Generic;
using System.Security.Cryptography;

public class Aes128CounterMode : SymmetricAlgorithm
{
    private readonly byte[] Counter;

    private readonly AesManaged AESCtr; public Aes128CounterMode(byte[] CTR)
    {
        AESCtr = new AesManaged
        {
            Mode = CipherMode.ECB,
            Padding = PaddingMode.None
        };
        Counter = CTR;
    }

    public override ICryptoTransform CreateEncryptor(byte[] Key, byte[] Input)
    {
        return new CryptoTransform(AESCtr, Key, Counter);
    }

    public override ICryptoTransform CreateDecryptor(byte[] Key, byte[] Input)
    {
        return new CryptoTransform(AESCtr, Key, Counter);
    }

    public override void GenerateKey()
    {
        AESCtr.GenerateKey();
    }

    public override void GenerateIV()
    {
    }
}

public class CryptoTransform : ICryptoTransform
{
    private readonly byte[] Counter;
    private readonly ICryptoTransform Transform;
    private readonly Queue<byte> QueuedBytes = new Queue<byte>();
    private readonly SymmetricAlgorithm Algorithm;

    public CryptoTransform(SymmetricAlgorithm Alg, byte[] Key, byte[] Counter)
    {
        Algorithm = Alg;
        this.Counter = Counter;
        var Block = new byte[Algorithm.BlockSize / 8];
        Transform = Alg.CreateEncryptor(Key, Block);
    }

    public byte[] TransformFinalBlock(byte[] InputData, int StartOffset, int Length)
    {
        var EncryptedData = new byte[Length];
        TransformBlock(InputData, StartOffset, Length, EncryptedData, 0);
        return EncryptedData;
    }

    public int TransformBlock(byte[] InputData, int StartOffset, int Length, byte[] Output, int OutOffset)
    {
        for (var i = 0; i < Length; i++)
        {
            if (Null()) CounterTransform(); var TransformBytes = QueuedBytes.Dequeue();
            Output[OutOffset + i] = (byte)(InputData[StartOffset + i] ^ TransformBytes);
        }
        return Length;
    }

    private bool Null()
    {
        return QueuedBytes.Count == 0;
    }

    private void CounterTransform()
    {
        var Output = new byte[Algorithm.BlockSize / 8];
        Transform.TransformBlock(Counter, 0, Counter.Length, Output, 0);
        Countup();
        foreach (var Byte in Output)
        {
            QueuedBytes.Enqueue(Byte);
        }
    }

    private void Countup()
    {
        for (var i = Counter.Length - 1; i >= 0; i--)
        {
            if (++Counter[i] != 0) break;
        }
    }

    public int InputBlockSize
    {
        get
        {
            return Algorithm.BlockSize / 8;
        }
    }

    public int OutputBlockSize
    {
        get
        {
            return Algorithm.BlockSize / 8;
        }
    }

    public bool CanTransformMultipleBlocks
    {
        get
        {
            return true;
        }
    }

    public bool CanReuseTransform
    {
        get
        {
            return false;
        }
    }

    public void Dispose()
    {
    }
}