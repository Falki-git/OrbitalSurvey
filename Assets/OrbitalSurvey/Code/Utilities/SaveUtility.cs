using System.IO.Compression;

namespace OrbitalSurvey.Utilities;

public static class SaveUtility
{
    public static string CompressData(bool[,] array)
    {
        using (var stream = new MemoryStream())
        {
            using (var gzipStream = new GZipStream(stream, CompressionMode.Compress))
            {
                using (var writer = new BinaryWriter(gzipStream))
                {
                    writer.Write(array.GetLength(0));
                    writer.Write(array.GetLength(1));

                    for (int i = 0; i < array.GetLength(0); i++)
                    {
                        for (int j = 0; j < array.GetLength(1); j++)
                        {
                            writer.Write(array[i,j]);
                        }
                    }
                }
            }

            // Convert compressed data to Base64
            string base64Data = Convert.ToBase64String(stream.ToArray());

            return base64Data;
        }
    }
    
    public static bool[,] DecompressData(string compressedString)
    {
        // Convert Base64 to byte array
        byte[] compressedBytes = Convert.FromBase64String(compressedString);

        using (var stream = new MemoryStream(compressedBytes))
        {
            using (var gzipStream = new GZipStream(stream, CompressionMode.Decompress))
            {
                using (var reader = new BinaryReader(gzipStream))
                {
                    // Read array dimensions
                    int width = reader.ReadInt32();
                    int height = reader.ReadInt32();

                    bool[,] decompressedArray = new bool[width, height];

                    for (int i = 0; i < width; i++)
                    {
                        for (int j = 0; j < height; j++)
                        {
                            decompressedArray[i, j] = reader.ReadBoolean();
                        }
                    }

                    return decompressedArray;
                }
            }
        }
    }

    public static bool[,] CopyArrayData(bool[,] sourceArray, out int truePixels)
    {
        bool[,] destinationArray = new bool[sourceArray.GetLength(0), sourceArray.GetLength(1)];
        truePixels = 0;
        
        int rows = sourceArray.GetLength(0);
        int columns = sourceArray.GetLength(1);

        // Flatten the arrays to 1D arrays for copying
        bool[] flattenedSource = new bool[rows * columns];
        bool[] flattenedDestination = new bool[rows * columns];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                flattenedSource[i * columns + j] = sourceArray[i, j];
                
                if (sourceArray[i, j])
                    truePixels++;
            }
        }

        // Use Array.Copy for fast copying
        Array.Copy(flattenedSource, flattenedDestination, flattenedSource.Length);

        // Restore the flattened array to a 2D array
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                destinationArray[i, j] = flattenedDestination[i * columns + j];
            }
        }

        return destinationArray;
    }
}