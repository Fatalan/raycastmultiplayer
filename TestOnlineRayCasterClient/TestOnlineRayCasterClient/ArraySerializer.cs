using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOnlineRayCasterClient
{
    internal class ArraySerializer
    {
            public static void SerializeMultidimensionalArray(NetDataWriter writer, int[,] array)
            {
                // Get dimensions
                int dim1 = array.GetLength(0);
                int dim2 = array.GetLength(1);

                // Write dimensions
                writer.Put(dim1);
                writer.Put(dim2);

                // Write elements
                for (int i = 0; i < dim1; i++)
                {
                    for (int j = 0; j < dim2; j++)
                    {
                        writer.Put(array[i, j]);
                    }
                }
            }

            public static int[,] DeserializeMultidimensionalArray(NetDataReader reader)
            {
                // Read dimensions
                int dim1 = reader.GetInt();
                int dim2 = reader.GetInt();

                // Create array
                int[,] array = new int[dim1, dim2];

                // Read elements
                for (int i = 0; i < dim1; i++)
                {
                    for (int j = 0; j < dim2; j++)
                    {
                        array[i, j] = reader.GetInt();
                    }
                }

                return array;
            }
    }
}
