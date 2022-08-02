using System;
using System.Threading.Tasks;
using UnityEngine;

//Modified from https://github.com/MaxxWyndham/LibSquishNet
namespace Squish
{
    [Flags]
    public enum SquishFlags
    {
        //! Use DXT1 compression.
        kDxt1 = 1,

        //! Use DXT3 compression.
        kDxt3 = 2,

        //! Use DXT5 compression.
        kDxt5 = 4,

        //! Use a very slow but very high quality colour compressor.
        kColourIterativeClusterFit = 256,

        //! Use a slow but high quality colour compressor (the default).
        kColourClusterFit = 8,

        //! Use a fast but low quality colour compressor.
        kColourRangeFit = 16,

        //! Weight the colour by alpha during cluster fit (disabled by default).
        kWeightColourByAlpha = 128
    };

    public static class Squish
    {
        public static bool HasFlag(this Enum variable, Enum value)
        {
            // check if from the same type.
            if (variable.GetType() != value.GetType())
            {
                throw new ArgumentException("The checked flag is not from the same type as the checked variable.");
            }

            Convert.ToUInt64(value);
            ulong num = Convert.ToUInt64(value);
            ulong num2 = Convert.ToUInt64(variable);

            return (num2 & num) == num;
        }

        private static SquishFlags fixFlags(SquishFlags flags)
        {
            // grab the flag bits
            SquishFlags method = flags & (SquishFlags.kDxt1 | SquishFlags.kDxt3 | SquishFlags.kDxt5);
            SquishFlags fit = flags & (SquishFlags.kColourIterativeClusterFit | SquishFlags.kColourClusterFit | SquishFlags.kColourRangeFit);
            SquishFlags extra = flags & SquishFlags.kWeightColourByAlpha;

            // set defaults
            if (method != SquishFlags.kDxt3 && method != SquishFlags.kDxt5) { method = SquishFlags.kDxt1; }
            if (fit != SquishFlags.kColourRangeFit && fit != SquishFlags.kColourIterativeClusterFit) { fit = SquishFlags.kColourClusterFit; }

            // done
            return method | fit | extra;
        }

        //public static int GetStorageRequirements(int width, int height, SquishFlags flags)
        //{
        //    // fix any bad flags
        //    flags = fixFlags(flags);

        //    // compute the storage requirements
        //    int blockcount = (width + 3) / 4 * ((height + 3) / 4);
        //    int blocksize = flags.HasFlag(SquishFlags.kDxt1) ? 8 : 16;

        //    return blockcount * blocksize;
        //}

        public static void DecompressImage(byte[] rgba, int width, int height, byte[] blocks, SquishFlags flags)
        {
            // fix any bad flags
            flags = fixFlags(flags);

            // initialise the block input
            int sourceBlock = 0;
            int bytesPerBlock = flags.HasFlag(SquishFlags.kDxt1) ? 8 : 16;

            // loop over blocks
            for (int y = 0; y < height; y += 4)
            {
                for (int x = 0; x < width; x += 4)
                {
                    // decompress the block
                    byte[] targetRgba = new byte[4 * 16];
                    decompress(targetRgba, blocks, sourceBlock, flags);

                    // write the decompressed pixels to the correct image locations
                    int sourcePixel = 0;

                    for (int py = 0; py < 4; ++py)
                    {
                        for (int px = 0; px < 4; ++px)
                        {
                            // get the target location
                            int sx = x + px;
                            int sy = y + py;

                            if (sx < width && sy < height)
                            {
                                int targetPixel = 4 * (width * sy + sx);

                                // copy the rgba value
                                for (int i = 0; i < 4; ++i)
                                {
                                    rgba[targetPixel] = targetRgba[sourcePixel];

                                    targetPixel++;
                                    sourcePixel++;
                                }
                            }
                            else
                            {
                                // skip this pixel as its outside the image
                                sourcePixel += 4;
                            }
                        }
                    }

                    // advance
                    sourceBlock += bytesPerBlock;
                }
            }
        }

        private static void decompress(byte[] rgba, byte[] block, int offset, SquishFlags flags)
        {
            // fix any bad flags
            flags = fixFlags(flags);

            // get the block locations
            int colourBlock = offset;
            int alphaBlock = offset;
            if (flags.HasFlag(SquishFlags.kDxt3) | flags.HasFlag(SquishFlags.kDxt5)) { colourBlock += 8; }

            // decompress colour
            ColourBlock.DecompressColour(rgba, block, colourBlock, flags.HasFlag(SquishFlags.kDxt1));

            // decompress alpha separately if necessary
            if (flags.HasFlag(SquishFlags.kDxt3))
            {
                throw new NotImplementedException("Squish.DecompressAlphaDxt3");
                //DecompressAlphaDxt3(rgba, alphaBlock);
            }
            else if (flags.HasFlag(SquishFlags.kDxt5))
            {
                decompressAlphaDxt5(rgba, block, alphaBlock);
            }
        }

        public static void CompressImage(byte[] rgba, int width, int height, byte[] blocks, SquishFlags flags, bool parallel = false)
        {
            // fix any bad flags
            flags = fixFlags(flags);

            // initialise the block output
            int targetBlock = 0;
            int bytesPerBlock = flags.HasFlag(SquishFlags.kDxt1) ? 8 : 16;

            if (parallel)
            {
                // loop over blocks
                Parallel.For(0, height / 4, (y) =>
                {
                    Parallel.For(0, width / 4, (x) =>
                    {
                        // build the 4x4 block of pixels
                        byte[] sourceRgba = new byte[16 * 4];
                        byte targetPixel = 0;
                        int mask = 0;

                        for (int py = 0; py < 4; ++py)
                        {
                            for (int px = 0; px < 4; ++px)
                            {
                                // get the source pixel in the image
                                int sx = x * 4 + px;
                                int sy = y * 4 + py;

                                // enable if we're in the image
                                if (sx < width && sy < height)
                                {
                                    // copy the rgba value
                                    for (int i = 0; i < 4; ++i)
                                    {
                                        sourceRgba[targetPixel] = rgba[i + 4 * (width * sy + sx)];
                                        targetPixel++;
                                    }

                                    // enable this pixel
                                    mask |= (1 << (4 * py + px));
                                }
                                else
                                {
                                    // skip this pixel as its outside the image
                                    targetPixel += 4;
                                }
                            }
                        }

                        // compress it into the output
                        compressMasked(sourceRgba, mask, blocks, (y * width / 4 * bytesPerBlock) + (x * bytesPerBlock), flags, null);
                    });
                });
            }
            else
            {
                // loop over blocks
                for (int y = 0; y < height; y += 4)
                {
                    for (int x = 0; x < width; x += 4)
                    {
                        // build the 4x4 block of pixels
                        byte[] sourceRgba = new byte[16 * 4];
                        byte targetPixel = 0;
                        int mask = 0;

                        for (int py = 0; py < 4; ++py)
                        {
                            for (int px = 0; px < 4; ++px)
                            {
                                // get the source pixel in the image
                                int sx = x + px;
                                int sy = y + py;

                                // enable if we're in the image
                                if (sx < width && sy < height)
                                {
                                    // copy the rgba value
                                    for (int i = 0; i < 4; ++i)
                                    {
                                        sourceRgba[targetPixel] = rgba[i + 4 * (width * sy + sx)];
                                        targetPixel++;
                                    }

                                    // enable this pixel
                                    mask |= (1 << (4 * py + px));
                                }
                                else
                                {
                                    // skip this pixel as its outside the image
                                    targetPixel += 4;
                                }
                            }
                        }

                        // compress it into the output
                        compressMasked(sourceRgba, mask, blocks, targetBlock, flags, null);

                        // advance
                        targetBlock += bytesPerBlock;
                    }
                }
            }
        }

        private static void compressMasked(byte[] rgba, int mask, byte[] block, int offset, SquishFlags flags, float? metric)
        {
            // fix any bad flags
            flags = fixFlags(flags);

            // get the block locations
            int colourBlock = offset;
            int alphaBlock = offset;
            if ((flags & (SquishFlags.kDxt3 | SquishFlags.kDxt5)) != 0) { colourBlock += 8; }

            // create the minimal point set
            ColourSet colours = new ColourSet(rgba, mask, flags);

            // check the compression type and compress colour
            if (colours.Count == 1)
            {
                // always do a single colour fit
                SingleColourFit fit = new SingleColourFit(colours, flags);
                fit.Compress(block, colourBlock);
            }
            else if ((flags & SquishFlags.kColourRangeFit) != 0 || colours.Count == 0)
            {
                // do a range fit
                RangeFit fit = new RangeFit(colours, flags, metric);
                fit.Compress(block, colourBlock);
            }
            else
            {
                // default to a cluster fit (could be iterative or not)
                ClusterFit fit = new ClusterFit(colours, flags, metric);
                fit.Compress(block, colourBlock);
            }

            // compress alpha separately if necessary
            if ((flags & SquishFlags.kDxt3) != 0)
            {
                compressAlphaDxt3(rgba, mask, block, alphaBlock);
            }
            else if ((flags & SquishFlags.kDxt5) != 0)
            {
                compressAlphaDxt5(rgba, mask, block, alphaBlock);
            }
        }

        private static void compressAlphaDxt3(byte[] rgba, int mask, byte[] block, int offset)
        {
            // quantise and pack the alpha values pairwise
            for (int i = 0; i < 8; ++i)
            {
                // quantise down to 4 bits
                float alpha1 = (float)rgba[8 * i + 3] * (15.0f / 255.0f);
                float alpha2 = (float)rgba[8 * i + 7] * (15.0f / 255.0f);
                int quant1 = ColourBlock.FloatToInt(alpha1, 15);
                int quant2 = ColourBlock.FloatToInt(alpha2, 15);

                // set alpha to zero where masked
                int bit1 = 1 << (2 * i);
                int bit2 = 1 << (2 * i + 1);
                if ((mask & bit1) == 0) { quant1 = 0; }
                if ((mask & bit2) == 0) { quant2 = 0; }

                // pack into the byte
                block[i + offset] = (byte)(quant1 | (quant2 << 4));
            }
        }

        private static void fixRange(int min, int max, int steps)
        {
            if (max - min < steps) { max = Math.Min(min + steps, 255); }
            if (max - min < steps) { min = Math.Max(0, max - steps); }
        }

        private static int fitCodes(byte[] rgba, int mask, byte[] codes, byte[] indices)
        {
            // fit each alpha value to the codebook
            int err = 0;

            for (int i = 0; i < 16; ++i)
            {
                // check this pixel is valid
                int bit = 1 << i;

                if ((mask & bit) == 0)
                {
                    // use the first code
                    indices[i] = 0;
                    continue;
                }

                // find the least error and corresponding index
                int value = rgba[4 * i + 3];
                int least = int.MaxValue;
                int index = 0;

                for (int j = 0; j < 8; ++j)
                {
                    // get the squared error from this code
                    int dist = (int)value - (int)codes[j];
                    dist *= dist;

                    // compare with the best so far
                    if (dist < least)
                    {
                        least = dist;
                        index = j;
                    }
                }

                // save this index and accumulate the error
                indices[i] = (byte)index;
                err += least;
            }

            // return the total error
            return err;
        }

        private static void writeAlphaBlock(int alpha0, int alpha1, byte[] indices, byte[] block, int offset)
        {
            // write the first two bytes
            block[offset + 0] = (byte)alpha0;
            block[offset + 1] = (byte)alpha1;

            // pack the indices with 3 bits each
            int dest = offset + 2;
            int src = 0;

            for (int i = 0; i < 2; ++i)
            {
                // pack 8 3-bit values
                int value = 0;

                for (int j = 0; j < 8; ++j)
                {
                    int index = indices[src];

                    value |= index << 3 * j;
                    src++;
                }

                // store in 3 bytes
                for (int j = 0; j < 3; ++j)
                {
                    int b = (value >> 8 * j) & 0xff;

                    block[dest] = (byte)b;
                    dest++;
                }
            }
        }

        private static void writeAlphaBlock5(int alpha0, int alpha1, byte[] indices, byte[] block, int offset)
        {
            // check the relative values of the endpoints
            if (alpha0 > alpha1)
            {
                // swap the indices
                byte[] swapped = new byte[16];

                for (int i = 0; i < 16; ++i)
                {
                    byte index = indices[i];

                    if (index == 0)
                    {
                        swapped[i] = 1;
                    }
                    else if (index == 1)
                    {
                        swapped[i] = 0;
                    }
                    else if (index <= 5)
                    {
                        swapped[i] = (byte)(7 - index);
                    }
                    else
                    {
                        swapped[i] = index;
                    }
                }

                // write the block
                writeAlphaBlock(alpha1, alpha0, swapped, block, offset);
            }
            else
            {
                // write the block
                writeAlphaBlock(alpha0, alpha1, indices, block, offset);
            }
        }

        private static void writeAlphaBlock7(int alpha0, int alpha1, byte[] indices, byte[] block, int offset)
        {
            // check the relative values of the endpoints
            if (alpha0 < alpha1)
            {
                // swap the indices
                byte[] swapped = new byte[16];

                for (int i = 0; i < 16; ++i)
                {
                    byte index = indices[i];

                    if (index == 0)
                    {
                        swapped[i] = 1;
                    }
                    else if (index == 1)
                    {
                        swapped[i] = 0;
                    }
                    else
                    {
                        swapped[i] = (byte)(9 - index);
                    }
                }

                // write the block
                writeAlphaBlock(alpha1, alpha0, swapped, block, offset);
            }
            else
            {
                // write the block
                writeAlphaBlock(alpha0, alpha1, indices, block, offset);
            }
        }

        private static void compressAlphaDxt5(byte[] rgba, int mask, byte[] block, int offset)
        {
            // get the range for 5-alpha and 7-alpha interpolation
            int min5 = 255;
            int max5 = 0;
            int min7 = 255;
            int max7 = 0;

            for (int i = 0; i < 16; ++i)
            {
                // check this pixel is valid
                int bit = 1 << i;

                if ((mask & bit) == 0) { continue; }

                // incorporate into the min/max
                int value = rgba[4 * i + 3];
                if (value < min7) { min7 = value; }
                if (value > max7) { max7 = value; }
                if (value != 0 && value < min5) { min5 = value; }
                if (value != 255 && value > max5) { max5 = value; }
            }

            // handle the case that no valid range was found
            if (min5 > max5) { min5 = max5; }
            if (min7 > max7) { min7 = max7; }

            // fix the range to be the minimum in each case
            fixRange(min5, max5, 5);
            fixRange(min7, max7, 7);

            // set up the 5-alpha code book
            byte[] codes5 = new byte[8];

            codes5[0] = (byte)min5;
            codes5[1] = (byte)max5;

            for (int i = 1; i < 5; ++i) { codes5[1 + i] = (byte)(((5 - i) * min5 + i * max5) / 5); }

            codes5[6] = 0;
            codes5[7] = 255;

            // set up the 7-alpha code book
            byte[] codes7 = new byte[8];

            codes7[0] = (byte)min7;
            codes7[1] = (byte)max7;

            for (int i = 1; i < 7; ++i) { codes7[1 + i] = (byte)(((7 - i) * min7 + i * max7) / 7); }

            // fit the data to both code books
            byte[] indices5 = new byte[16];
            byte[] indices7 = new byte[16];
            int err5 = fitCodes(rgba, mask, codes5, indices5);
            int err7 = fitCodes(rgba, mask, codes7, indices7);

            // save the block with least error
            if (err5 <= err7)
            {
                writeAlphaBlock5(min5, max5, indices5, block, offset);
            }
            else
            {
                writeAlphaBlock7(min7, max7, indices7, block, offset);
            }
        }

        private static void decompressAlphaDxt5(byte[] rgba, byte[] block, int offset)
        {
            // get the two alpha values
            int alpha0 = block[offset + 0];
            int alpha1 = block[offset + 1];

            // compare the values to build the codebook
            byte[] codes = new byte[8];
            codes[0] = (byte)alpha0;
            codes[1] = (byte)alpha1;

            if (alpha0 <= alpha1)
            {
                // use 5-alpha codebook
                for (int i = 1; i < 5; ++i)
                {
                    codes[1 + i] = (byte)(((5 - i) * alpha0 + i * alpha1) / 5);
                }
                codes[6] = 0;
                codes[7] = 255;
            }
            else
            {
                // use 7-alpha codebook
                for (int i = 1; i < 7; ++i)
                {
                    codes[1 + i] = (byte)(((7 - i) * alpha0 + i * alpha1) / 7);
                }
            }

            // decode the indices
            byte[] indices = new byte[16];
            int src = offset + 2;
            int dest = 0;

            for (int i = 0; i < 2; ++i)
            {
                // grab 3 bytes
                int value = 0;
                for (int j = 0; j < 3; ++j)
                {
                    int b = block[src++];
                    value |= (b << 8 * j);
                }

                // unpack 8 3-bit values from it
                for (int j = 0; j < 8; ++j)
                {
                    int index = (value >> 3 * j) & 0x7;
                    indices[dest++] = (byte)index;
                }
            }

            // write out the indexed codebook values
            for (int i = 0; i < 16; ++i)
            {
                rgba[4 * i + 3] = codes[indices[i]];
            }
        }
    }

    public class ClusterFit : ColourFit
    {
        private int IterationCount { get; set; }

        private Vector3 Principle { get; set; }

        private byte[] Order { get; set; } = new byte[16 * 8];

        private Vector4[] PointsWeights { get; set; } = new Vector4[16];

        private Vector4 XsumWsum { get; set; }

        private Vector4 Metric { get; set; }

        private Vector4 BestError { get; set; }

        public ClusterFit(ColourSet colours, SquishFlags flags, float? metric)
            : base(colours, flags)
        {
            // set the iteration count
            IterationCount = (Flags & SquishFlags.kColourIterativeClusterFit) != 0 ? 8 : 1;

            // initialise the metric (old perceptual = 0.2126f, 0.7152f, 0.0722f)
            if (metric != null)
            {
                //m_metric = Vec4( metric[0], metric[1], metric[2], 1.0f );
            }
            else
            {
                Metric = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            }

            // initialise the best error
            BestError = new Vector4(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue);

            // cache some values
            int count = Colours.Count;
            Vector3[] values = Colours.Points;

            // get the covariance matrix
            Sym3x3 covariance = Sym3x3.ComputeWeightedCovariance(count, values, Colours.Weights);

            // compute the principle component
            Principle = Sym3x3.ComputePrincipleComponent(covariance);
        }

        public bool ConstructOrdering(Vector3 axis, int iteration)
        {
            // cache some values
            int count = Colours.Count;
            Vector3[] values = Colours.Points;

            // build the list of dot products
            float[] dps = new float[16];

            for (int i = 0; i < count; ++i)
            {
                dps[i] = Vector3.Dot(values[i], axis);
                Order[(16 * iteration) + i] = (byte)i;
            }

            // stable sort using them
            for (int i = 0; i < count; ++i)
            {
                for (int j = i; j > 0 && dps[j] < dps[j - 1]; --j)
                {
                    float tf = dps[j];
                    dps[j] = dps[j - 1];
                    dps[j - 1] = tf;

                    byte tb = Order[(16 * iteration) + j];
                    Order[(16 * iteration) + j] = Order[(16 * iteration) + j - 1];
                    Order[(16 * iteration) + j - 1] = tb;
                }
            }

            // check this ordering is unique
            for (int it = 0; it < iteration; ++it)
            {
                bool same = true;

                for (int i = 0; i < count; ++i)
                {
                    if (Order[(16 * iteration) + i] != Order[(16 * it) + i])
                    {
                        same = false;
                        break;
                    }
                }

                if (same) { return false; }
            }

            // copy the ordering and weight all the points
            Vector3[] unweighted = Colours.Points;
            float[] weights = Colours.Weights;

            XsumWsum = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

            for (int i = 0; i < count; ++i)
            {
                int j = Order[(16 * iteration) + i];
                Vector4 p = new Vector4(unweighted[j].x, unweighted[j].y, unweighted[j].z, 1.0f);
                Vector4 w = new Vector4(weights[j], weights[j], weights[j], weights[j]);
                Vector4 x = Vector4.Scale(p, w);

                PointsWeights[i] = x;
                XsumWsum += x;
            }

            return true;
        }

        public override void Compress3(byte[] block, int offset)
        {
            // declare variables
            int count = Colours.Count;
            Vector4 two = new Vector4(2.0f, 2.0f, 2.0f, 2.0f);
            Vector4 one = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            Vector4 half_half2 = new Vector4(0.5f, 0.5f, 0.5f, 0.25f);
            Vector4 zero = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            Vector4 half = new Vector4(0.5f, 0.5f, 0.5f, 0.5f);
            Vector4 grid = new Vector4(31.0f, 63.0f, 31.0f, 0.0f);
            Vector4 gridrcp = new Vector4(1.0f / 31.0f, 1.0f / 63.0f, 1.0f / 31.0f, 0.0f);

            // prepare an ordering using the principle axis
            ConstructOrdering(Principle, 0);

            // check all possible clusters and iterate on the total order
            Vector4 beststart = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            Vector4 bestend = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            Vector4 besterror = BestError;
            byte[] bestindices = new byte[16];
            int bestiteration = 0;
            int besti = 0, bestj = 0;

            // loop over iterations (we avoid the case that all points in first or last cluster)
            for (int iterationIndex = 0; ;)
            {
                // first cluster [0,i) is at the start
                Vector4 part0 = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
                for (int i = 0; i < count; ++i)
                {
                    // second cluster [i,j) is half along
                    Vector4 part1 = (i == 0) ? PointsWeights[0] : new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
                    int jmin = (i == 0) ? 1 : i;
                    for (int j = jmin; ;)
                    {
                        // last cluster [j,count) is at the end
                        Vector4 part2 = XsumWsum - part1 - part0;

                        // compute least squares terms directly
                        Vector4 alphax_sum = Helpers.MultiplyAdd(part1, half_half2, part0);
                        Vector4 alpha2_sum = alphax_sum.SplatW();

                        Vector4 betax_sum = Helpers.MultiplyAdd(part1, half_half2, part2);
                        Vector4 beta2_sum = betax_sum.SplatW();

                        Vector4 alphabeta_sum = (Vector4.Scale(part1, half_half2)).SplatW();

                        // compute the least-squares optimal points
                        Vector4 factor = Helpers.Reciprocal(Helpers.NegativeMultiplySubtract(alphabeta_sum, alphabeta_sum, Vector4.Scale(alpha2_sum , beta2_sum)));
                        Vector4 a = Vector4.Scale(Helpers.NegativeMultiplySubtract(betax_sum, alphabeta_sum, Vector4.Scale(alphax_sum , beta2_sum)) , factor);
                        Vector4 b = Vector4.Scale(Helpers.NegativeMultiplySubtract(alphax_sum, alphabeta_sum, Vector4.Scale(betax_sum , alpha2_sum)) , factor);

                        // clamp to the grid
                        a = Vector4.Min(one, Vector4.Max(zero, a));
                        b = Vector4.Min(one, Vector4.Max(zero, b));
                        a = Vector4.Scale(Helpers.Truncate(Helpers.MultiplyAdd(grid, a, half)), gridrcp);
                        b = Vector4.Scale(Helpers.Truncate(Helpers.MultiplyAdd(grid, b, half)), gridrcp);

                        // compute the error (we skip the constant xxsum)
                        Vector4 e1 = Helpers.MultiplyAdd(Vector4.Scale(a , a), alpha2_sum, Vector4.Scale(Vector4.Scale(b , b) , beta2_sum));
                        Vector4 e2 = Helpers.NegativeMultiplySubtract(a, alphax_sum, Vector4.Scale(Vector4.Scale(a , b) , alphabeta_sum));
                        Vector4 e3 = Helpers.NegativeMultiplySubtract(b, betax_sum, e2);
                        Vector4 e4 = Helpers.MultiplyAdd(two, e3, e1);

                        // apply the metric to the error term
                        Vector4 e5 = Vector4.Scale(e4 , Metric);
                        Vector4 error = e5.SplatX() + e5.SplatY() + e5.SplatZ();

                        // keep the solution if it wins
                        if (Helpers.CompareAnyLessThan(error, besterror))
                        {
                            beststart = a;
                            bestend = b;
                            besti = i;
                            bestj = j;
                            besterror = error;
                            bestiteration = iterationIndex;
                        }

                        // advance
                        if (j == count) { break; }

                        part1 += PointsWeights[j];
                        ++j;
                    }

                    // advance
                    part0 += PointsWeights[i];
                }

                // stop if we didn't improve in this iteration
                if (bestiteration != iterationIndex) { break; }

                // advance if possible
                ++iterationIndex;

                if (iterationIndex == IterationCount) { break; }

                // stop if a new iteration is an ordering that has already been tried
                Vector3 axis = (bestend - beststart).ToVector3();

                if (!ConstructOrdering(axis, iterationIndex)) { break; }
            }

            // save the block if necessary
            if (Helpers.CompareAnyLessThan(besterror, BestError))
            {
                byte[] unordered = new byte[16];

                for (int m = 0; m < besti; ++m)
                {
                    unordered[Order[(16 * bestiteration) + m]] = 0;
                }

                for (int m = besti; m < bestj; ++m)
                {
                    unordered[Order[(16 * bestiteration) + m]] = 2;
                }

                for (int m = bestj; m < count; ++m)
                {
                    unordered[Order[(16 * bestiteration) + m]] = 1;
                }

                Colours.RemapIndices(unordered, bestindices);

                // save the block
                ColourBlock.WriteColourBlock3(beststart.ToVector3(), bestend.ToVector3(), bestindices, block, offset);

                // save the error
                BestError = besterror;
            }
        }

        public override void Compress4(byte[] block, int offset)
        {
            // declare variables
            int count = Colours.Count;
            Vector4 two = new Vector4(2.0f, 2.0f, 2.0f, 2.0f);
            Vector4 one = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            Vector4 onethird_onethird2 = new Vector4(1.0f / 3.0f, 1.0f / 3.0f, 1.0f / 3.0f, 1.0f / 9.0f);
            Vector4 twothirds_twothirds2 = new Vector4(2.0f / 3.0f, 2.0f / 3.0f, 2.0f / 3.0f, 4.0f / 9.0f);
            Vector4 twonineths = new Vector4(2.0f / 9.0f, 2.0f / 9.0f, 2.0f / 9.0f, 2.0f / 9.0f);
            Vector4 zero = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            Vector4 half = new Vector4(0.5f, 0.5f, 0.5f, 0.5f);
            Vector4 grid = new Vector4(31.0f, 63.0f, 31.0f, 0.0f);
            Vector4 gridrcp = new Vector4(1.0f / 31.0f, 1.0f / 63.0f, 1.0f / 31.0f, 0.0f);

            // prepare an ordering using the principle axis
            ConstructOrdering(Principle, 0);

            // check all possible clusters and iterate on the total order
            Vector4 beststart = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            Vector4 bestend = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            Vector4 besterror = BestError;
            byte[] bestindices = new byte[16];
            int bestiteration = 0;
            int besti = 0, bestj = 0, bestk = 0;

            // loop over iterations (we avoid the case that all points in first or last cluster)
            for (int iterationIndex = 0; ;)
            {
                // first cluster [0,i) is at the start
                Vector4 part0 = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
                for (int i = 0; i < count; ++i)
                {
                    // second cluster [i,j) is one third along
                    Vector4 part1 = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
                    for (int j = i; ;)
                    {
                        // third cluster [j,k) is two thirds along
                        Vector4 part2 = (j == 0) ? PointsWeights[0] : new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
                        int kmin = (j == 0) ? 1 : j;
                        for (int k = kmin; ;)
                        {
                            // last cluster [k,count) is at the end
                            Vector4 part3 = XsumWsum - part2 - part1 - part0;

                            // compute least squares terms directly
                            Vector4 alphax_sum = Helpers.MultiplyAdd(part2, onethird_onethird2, Helpers.MultiplyAdd(part1, twothirds_twothirds2, part0));
                            Vector4 alpha2_sum = alphax_sum.SplatW();

                            Vector4 betax_sum = Helpers.MultiplyAdd(part1, onethird_onethird2, Helpers.MultiplyAdd(part2, twothirds_twothirds2, part3));
                            Vector4 beta2_sum = betax_sum.SplatW();

                            Vector4 alphabeta_sum = Vector4.Scale(twonineths , (part1 + part2).SplatW());

                            // compute the least-squares optimal points
                            Vector4 factor = Helpers.Reciprocal(Helpers.NegativeMultiplySubtract(alphabeta_sum, alphabeta_sum, Vector4.Scale(alpha2_sum , beta2_sum)));
                            Vector4 a = Vector4.Scale(Helpers.NegativeMultiplySubtract(betax_sum, alphabeta_sum, Vector4.Scale(alphax_sum , beta2_sum)) , factor);
                            Vector4 b = Vector4.Scale(Helpers.NegativeMultiplySubtract(alphax_sum, alphabeta_sum, Vector4.Scale(betax_sum , alpha2_sum)) , factor);

                            // clamp to the grid
                            a = Vector4.Min(one, Vector4.Max(zero, a));
                            b = Vector4.Min(one, Vector4.Max(zero, b));
                            a = Vector4.Scale(Helpers.Truncate(Helpers.MultiplyAdd(grid, a, half)) , gridrcp);
                            b = Vector4.Scale(Helpers.Truncate(Helpers.MultiplyAdd(grid, b, half)) , gridrcp);

                            // compute the error (we skip the constant xxsum)
                            Vector4 e1 = Helpers.MultiplyAdd(Vector4.Scale(a , a), alpha2_sum, Vector4.Scale(Vector4.Scale(b , b) , beta2_sum));
                            Vector4 e2 = Helpers.NegativeMultiplySubtract(a, alphax_sum, Vector4.Scale(Vector4.Scale(a , b) , alphabeta_sum));
                            Vector4 e3 = Helpers.NegativeMultiplySubtract(b, betax_sum, e2);
                            Vector4 e4 = Helpers.MultiplyAdd(two, e3, e1);

                            // apply the metric to the error term
                            Vector4 e5 = Vector4.Scale(e4 , Metric);
                            Vector4 error = e5.SplatX() + e5.SplatY() + e5.SplatZ();

                            // keep the solution if it wins
                            if (Helpers.CompareAnyLessThan(error, besterror))
                            {
                                beststart = a;
                                bestend = b;
                                besterror = error;
                                besti = i;
                                bestj = j;
                                bestk = k;
                                bestiteration = iterationIndex;
                            }

                            // advance
                            if (k == count) { break; }
                            part2 += PointsWeights[k];
                            ++k;
                        }

                        // advance
                        if (j == count) { break; }
                        part1 += PointsWeights[j];
                        ++j;
                    }

                    // advance
                    part0 += PointsWeights[i];
                }

                // stop if we didn't improve in this iteration
                if (bestiteration != iterationIndex) { break; }

                // advance if possible
                ++iterationIndex;
                if (iterationIndex == IterationCount) { break; }

                // stop if a new iteration is an ordering that has already been tried
                Vector3 axis = (bestend - beststart).ToVector3();
                if (!ConstructOrdering(axis, iterationIndex)) { break; }
            }

            // save the block if necessary
            if (Helpers.CompareAnyLessThan(besterror, BestError))
            {
                // remap the indices
                byte[] unordered = new byte[16];

                for (int m = 0; m < besti; ++m)
                {
                    unordered[Order[(16 * bestiteration) + m]] = 0;
                }

                for (int m = besti; m < bestj; ++m)
                {
                    unordered[Order[(16 * bestiteration) + m]] = 2;
                }

                for (int m = bestj; m < bestk; ++m)
                {
                    unordered[Order[(16 * bestiteration) + m]] = 3;
                }

                for (int m = bestk; m < count; ++m)
                {
                    unordered[Order[(16 * bestiteration) + m]] = 1;
                }

                Colours.RemapIndices(unordered, bestindices);

                // save the block
                ColourBlock.WriteColourBlock4(beststart.ToVector3(), bestend.ToVector3(), bestindices, block, offset);

                // save the error
                BestError = besterror;
            }
        }
    }
    public class ColourBlock
    {
        public static int FloatToInt(float a, int limit)
        {
            // use ANSI round-to-zero behaviour to get round-to-nearest
            int i = (int)(a + 0.5f);

            // clamp to the limit
            if (i < 0)
            {
                i = 0;
            }
            else if (i > limit)
            {
                i = limit;
            }

            // done
            return i;
        }

        private static int floatTo565(Vector3 colour)
        {
            // get the components in the correct range
            int r = FloatToInt(31.0f * colour.x, 31);
            int g = FloatToInt(63.0f * colour.y, 63);
            int b = FloatToInt(31.0f * colour.z, 31);

            // pack into a single value
            return (r << 11) | (g << 5) | b;
        }

        private static void writeColourBlock(int a, int b, byte[] indices, byte[] block, int offset)
        {
            // write the endpoints
            block[offset + 0] = (byte)(a & 0xff);
            block[offset + 1] = (byte)(a >> 8);
            block[offset + 2] = (byte)(b & 0xff);
            block[offset + 3] = (byte)(b >> 8);

            // write the indices
            for (int i = 0; i < 4; ++i)
            {
                block[offset + 4 + i] = (byte)(indices[(4 * i) + 0] | (indices[(4 * i) + 1] << 2) | (indices[(4 * i) + 2] << 4) | (indices[(4 * i) + 3] << 6));
            }
        }

        public static void WriteColourBlock3(Vector3 start, Vector3 end, byte[] indices, byte[] block, int offset)
        {
            // get the packed values
            int a = floatTo565(start);
            int b = floatTo565(end);

            // remap the indices
            byte[] remapped = new byte[16];
            if (a <= b)
            {
                // use the indices directly
                for (int i = 0; i < 16; ++i)
                {
                    remapped[i] = indices[i];
                }
            }
            else
            {
                // swap a and b
                int t = a;
                a = b;
                b = t;

                for (int i = 0; i < 16; ++i)
                {
                    if (indices[i] == 0)
                    {
                        remapped[i] = 1;
                    }
                    else if (indices[i] == 1)
                    {
                        remapped[i] = 0;
                    }
                    else
                    {
                        remapped[i] = indices[i];
                    }
                }
            }

            // write the block
            writeColourBlock(a, b, remapped, block, offset);
        }

        public static void WriteColourBlock4(Vector3 start, Vector3 end, byte[] indices, byte[] block, int offset)
        {
            // get the packed values
            int a = floatTo565(start);
            int b = floatTo565(end);

            // remap the indices
            byte[] remapped = new byte[16];
            if (a < b)
            {
                // swap a and b
                int t = a;
                a = b;
                b = t;

                for (int i = 0; i < 16; ++i)
                {
                    remapped[i] = (byte)((indices[i] ^ 0x1) & 0x3);
                }
            }
            else if (a == b)
            {
                // use index 0
                for (int i = 0; i < 16; ++i)
                {
                    remapped[i] = 0;
                }
            }
            else
            {
                // use the indices directly
                for (int i = 0; i < 16; ++i)
                {
                    remapped[i] = indices[i];
                }
            }

            // write the block
            writeColourBlock(a, b, remapped, block, offset);
        }

        private static int unpack565(byte[] packed, int offset, byte[] colour, int colouroffset)
        {
            // build the packed value
            int value = (int)packed[offset + 0] | ((int)packed[offset + 1] << 8);

            // get the components in the stored range
            byte red = (byte)((value >> 11) & 0x1f);
            byte green = (byte)((value >> 5) & 0x3f);
            byte blue = (byte)(value & 0x1f);

            // scale up to 8 bits
            colour[colouroffset + 0] = (byte)((red << 3) | (red >> 2));
            colour[colouroffset + 1] = (byte)((green << 2) | (green >> 4));
            colour[colouroffset + 2] = (byte)((blue << 3) | (blue >> 2));
            colour[colouroffset + 3] = 255;

            // return the value
            return value;
        }

        public static void DecompressColour(byte[] rgba, byte[] block, int offset, bool isDxt1)
        {
            // unpack the endpoints
            byte[] codes = new byte[16];
            int a = unpack565(block, offset, codes, 0);
            int b = unpack565(block, offset + 2, codes, 4);

            // generate the midpoints
            for (int i = 0; i < 3; ++i)
            {
                int c = codes[i];
                int d = codes[4 + i];

                if (isDxt1 && a <= b)
                {
                    codes[8 + i] = (byte)((c + d) / 2);
                    codes[12 + i] = 0;
                }
                else
                {
                    codes[8 + i] = (byte)((2 * c + d) / 3);
                    codes[12 + i] = (byte)((c + 2 * d) / 3);
                }
            }

            // fill in alpha for the intermediate values
            codes[8 + 3] = 255;
            codes[12 + 3] = (byte)(isDxt1 && a <= b ? 0 : 255);

            // unpack the indices
            byte[] indices = new byte[16];

            for (int i = 0; i < 4; ++i)
            {
                int ind = 4 * i;
                byte packed = block[offset + 4 + i];

                indices[ind + 0] = (byte)(packed & 0x3);
                indices[ind + 1] = (byte)((packed >> 2) & 0x3);
                indices[ind + 2] = (byte)((packed >> 4) & 0x3);
                indices[ind + 3] = (byte)((packed >> 6) & 0x3);
            }

            // store out the colours
            for (int i = 0; i < 16; ++i)
            {
                byte coffset = (byte)(4 * indices[i]);
                for (int j = 0; j < 4; ++j)
                {
                    rgba[4 * i + j] = codes[coffset + j];
                }
            }
        }
    }
    public class ColourFit
    {
        protected ColourSet Colours { get; set; }

        protected SquishFlags Flags { get; set; }

        public ColourFit(ColourSet colours, SquishFlags flags)
        {
            Colours = colours;
            Flags = flags;
        }

        public void Compress(byte[] block, int offset)
        {
            bool isDxt1 = Flags.HasFlag(SquishFlags.kDxt1);

            if (isDxt1)
            {
                Compress3(block, offset);

                if (!Colours.IsTransparent)
                {
                    Compress4(block, offset);
                }
            }
            else
            {
                Compress4(block, offset);
            }
        }

        public virtual void Compress3(byte[] block, int offset) { }

        public virtual void Compress4(byte[] block, int offset) { }
    }
    public class ColourSet
    {
        public int Count { get; set; } = 0;

        public Vector3[] Points { get; set; } = new Vector3[16];

        public float[] Weights { get; set; } = new float[16];

        private int[] Remap { get; set; } = new int[16];

        public bool IsTransparent { get; set; } = false;

        public ColourSet(byte[] rgba, int mask, SquishFlags flags)
        {
            // check the compression mode for dxt1
            bool isDxt1 = flags.HasFlag(SquishFlags.kDxt1);
            bool weightByAlpha = flags.HasFlag(SquishFlags.kWeightColourByAlpha);

            // create the minimal set
            for (int i = 0; i < 16; ++i)
            {
                // check this pixel is enabled
                int bit = 1 << i;

                if ((mask & bit) == 0)
                {
                    Remap[i] = -1;
                    continue;
                }

                // check for transparent pixels when using dxt1
                if (isDxt1 && rgba[4 * i + 3] < 128)
                {
                    Remap[i] = -1;
                    IsTransparent = true;

                    continue;
                }

                // loop over previous points for a match
                for (int j = 0; ; ++j)
                {
                    // allocate a new point
                    if (j == i)
                    {
                        // normalise coordinates to [0,1]
                        float x = rgba[4 * i] / 255.0f;
                        float y = rgba[4 * i + 1] / 255.0f;
                        float z = rgba[4 * i + 2] / 255.0f;

                        // ensure there is always non-zero weight even for zero alpha
                        float w = (rgba[4 * i + 3] + 1) / 256.0f;

                        // add the point
                        Points[Count] = new Vector3(x, y, z);
                        Weights[Count] = weightByAlpha ? w : 1.0f;
                        Remap[i] = Count;

                        // advance
                        ++Count;
                        break;
                    }

                    // check for a match
                    int oldbit = 1 << j;
                    bool match = ((mask & oldbit) != 0) &&
                        (rgba[4 * i] == rgba[4 * j]) &&
                        (rgba[4 * i + 1] == rgba[4 * j + 1]) &&
                        (rgba[4 * i + 2] == rgba[4 * j + 2]) &&
                        (rgba[4 * j + 3] >= 128 || !isDxt1);

                    if (match)
                    {
                        // get the index of the match
                        int index = Remap[j];

                        // ensure there is always non-zero weight even for zero alpha
                        float w = (rgba[4 * i + 3] + 1) / 256.0f;

                        // map to this point and increase the weight
                        Weights[index] += (weightByAlpha ? w : 1.0f);
                        Remap[i] = index;
                        break;
                    }
                }
            }

            // square root the weights
            for (int i = 0; i < Count; ++i)
            {
                Weights[i] = (float)Math.Sqrt(Weights[i]);
            }
        }

        public void RemapIndices(byte[] source, byte[] target)
        {
            for (int i = 0; i < 16; ++i)
            {
                int j = Remap[i];
                if (j == -1)
                {
                    target[i] = 3;
                }
                else
                {
                    target[i] = source[j];
                }
            }
        }
    }
    public class RangeFit : ColourFit
    {
        Vector3 m_metric;
        Vector3 m_start;
        Vector3 m_end;
        float m_besterror;

        public RangeFit(ColourSet colours, SquishFlags flags, float? metric)
            : base(colours, flags)
        {
            // initialise the metric (old perceptual = 0.2126f, 0.7152f, 0.0722f)
            if (metric != null)
            {
                //m_metric = new Vector3( metric[0], metric[1], metric[2] );
            }
            else
            {
                m_metric = new Vector3(1.0f, 1.0f, 1.0f);
            }

            // initialise the best error
            m_besterror = float.MaxValue;

            // cache some values
            int count = Colours.Count;
            Vector3[] values = Colours.Points;
            float[] weights = Colours.Weights;

            // get the covariance matrix
            Sym3x3 covariance = Sym3x3.ComputeWeightedCovariance(count, values, weights);

            // compute the principle component
            Vector3 principle = Sym3x3.ComputePrincipleComponent(covariance);

            // get the min and max range as the codebook endpoints
            Vector3 start = new Vector3(0.0f, 0.0f, 0.0f);
            Vector3 end = new Vector3(0.0f, 0.0f, 0.0f);
            if (count > 0)
            {
                float min, max;

                // compute the range
                start = end = values[0];
                min = max = Vector3.Dot(values[0], principle);
                for (int i = 1; i < count; ++i)
                {
                    float val = Vector3.Dot(values[i], principle);
                    if (val < min)
                    {
                        start = values[i];
                        min = val;
                    }
                    else if (val > max)
                    {
                        end = values[i];
                        max = val;
                    }
                }
            }

            // clamp the output to [0, 1]
            Vector3 one = new Vector3(1.0f, 1.0f, 1.0f);
            Vector3 zero = new Vector3(0.0f, 0.0f, 0.0f);
            start = Vector3.Min(one, Vector3.Max(zero, start));
            end = Vector3.Min(one, Vector3.Max(zero, end));

            // clamp to the grid and save
            Vector3 grid = new Vector3(31.0f, 63.0f, 31.0f);
            Vector3 gridrcp = new Vector3(1.0f / 31.0f, 1.0f / 63.0f, 1.0f / 31.0f);
            Vector3 half = new Vector3(0.5f, 0.5f, 0.5f);
            m_start = Vector3.Scale(Helpers.Truncate(Vector3.Scale(grid , start) + half), gridrcp);
            m_end = Vector3.Scale(Helpers.Truncate(Vector3.Scale(grid, end) + half), gridrcp);
        }

        public override void Compress3(byte[] block, int offset)
        {
            // cache some values
            int count = Colours.Count;
            Vector3[] values = Colours.Points;

            // create a codebook
            Vector3[] codes = new Vector3[3];
            codes[0] = m_start;
            codes[1] = m_end;
            codes[2] = 0.5f * m_start + 0.5f * m_end;

            // match each point to the closest code
            byte[] closest = new byte[16];
            float error = 0.0f;
            for (int i = 0; i < count; ++i)
            {
                // find the closest code
                float dist = float.MaxValue;
                int idx = 0;
                for (int j = 0; j < 3; ++j)
                {
                    float d = (Vector3.Scale(m_metric, (values[i] - codes[j]))).sqrMagnitude;
                    if (d < dist)
                    {
                        dist = d;
                        idx = j;
                    }
                }

                // save the index
                closest[i] = (byte)idx;

                // accumulate the error
                error += dist;
            }

            // save this scheme if it wins
            if (error < m_besterror)
            {
                // remap the indices
                byte[] indices = new byte[16];
                Colours.RemapIndices(closest, indices);

                // save the block
                ColourBlock.WriteColourBlock3(m_start, m_end, indices, block, offset);

                // save the error
                m_besterror = error;
            }
        }

        public override void Compress4(byte[] block, int offset)
        {
            throw new NotImplementedException("RangeFit.Compress4");
        }
    }
    public class SingleColourFit : ColourFit
    {
        byte[] m_colour = new byte[3];
        Vector3 m_start;
        Vector3 m_end;
        byte m_index;
        int m_error;
        int m_besterror;

        public SingleColourFit(ColourSet colours, SquishFlags flags)
            : base(colours, flags)
        {
            // grab the single colour
            Vector3[] values = Colours.Points;
            m_colour[0] = (byte)ColourBlock.FloatToInt(255.0f * values[0].x, 255);
            m_colour[1] = (byte)ColourBlock.FloatToInt(255.0f * values[0].y, 255);
            m_colour[2] = (byte)ColourBlock.FloatToInt(255.0f * values[0].z, 255);

            // initialise the best error
            m_besterror = int.MaxValue;
        }

        public void ComputeEndPoints(SingleColourLookup[][] lookups)
        {
            // check each index combination (endpoint or intermediate)
            m_error = int.MaxValue;
            for (int index = 0; index < 2; ++index)
            {
                // check the error for this codebook index
                SourceBlock[] sources = new SourceBlock[3];
                int error = 0;
                for (int channel = 0; channel < 3; ++channel)
                {
                    // grab the lookup table and index for this channel
                    SingleColourLookup[] lookup = lookups[channel];
                    int target = m_colour[channel];

                    // store a pointer to the source for this channel
                    sources[channel] = lookup[target].sources[index];

                    // accumulate the error
                    int diff = sources[channel].error;
                    error += diff * diff;
                }

                // keep it if the error is lower
                if (error < m_error)
                {
                    m_start = new Vector3(
                            (float)sources[0].start / 31.0f,
                            (float)sources[1].start / 63.0f,
                            (float)sources[2].start / 31.0f
                    );
                    m_end = new Vector3(
                            (float)sources[0].end / 31.0f,
                            (float)sources[1].end / 63.0f,
                            (float)sources[2].end / 31.0f
                    );
                    m_index = (byte)(2 * index);
                    m_error = error;
                }
            }
        }

        public override void Compress3(byte[] block, int offset)
        {
            // build the table of lookups
            SingleColourLookup[][] lookups = new SingleColourLookup[][]
            {
                SingleColourLookupIns.Lookup53,
                SingleColourLookupIns.Lookup63,
                SingleColourLookupIns.Lookup53
            };

            // find the best end-points and index
            ComputeEndPoints(lookups);

            // build the block if we win
            if (m_error < m_besterror)
            {
                // remap the indices
                byte[] indices = new byte[16];
                Colours.RemapIndices(new byte[] { m_index }, indices);

                // save the block
                ColourBlock.WriteColourBlock3(m_start, m_end, indices, block, offset);

                // save the error
                m_besterror = m_error;
            }
        }

        public override void Compress4(byte[] block, int offset)
        {
            // build the table of lookups
            SingleColourLookup[][] lookups = new SingleColourLookup[][]
            {
                SingleColourLookupIns.Lookup54,
                SingleColourLookupIns.Lookup64,
                SingleColourLookupIns.Lookup54
            };

            // find the best end-points and index
            ComputeEndPoints(lookups);

            // build the block if we win
            if (m_error < m_besterror)
            {
                // remap the indices
                byte[] indices = new byte[16];
                Colours.RemapIndices(new byte[] { m_index }, indices);

                // save the block
                ColourBlock.WriteColourBlock4(m_start, m_end, indices, block, offset);

                // save the error
                m_besterror = m_error;
            }
        }
    }
    public struct SingleColourLookup
    {
        public SourceBlock[] sources;

        public SingleColourLookup(SourceBlock a, SourceBlock b)
        {
            sources = new SourceBlock[] { a, b };
        }
    };
    public static class SingleColourLookupIns
    {
        public static SingleColourLookup[] Lookup53 = new SingleColourLookup[]
        {
            new SingleColourLookup(new SourceBlock(0, 0, 0), new SourceBlock(0, 0, 0)),
            new SingleColourLookup(new SourceBlock(0, 0, 1), new SourceBlock(0, 0, 1)),
            new SingleColourLookup(new SourceBlock(0, 0, 2), new SourceBlock(0, 0, 2)),
            new SingleColourLookup(new SourceBlock(0, 0, 3), new SourceBlock(0, 1, 1)),
            new SingleColourLookup(new SourceBlock(0, 0, 4), new SourceBlock(0, 1, 0)),
            new SingleColourLookup(new SourceBlock(1, 0, 3), new SourceBlock(0, 1, 1)),
            new SingleColourLookup(new SourceBlock(1, 0, 2), new SourceBlock(0, 1, 2)),
            new SingleColourLookup(new SourceBlock(1, 0, 1), new SourceBlock(0, 2, 1)),
            new SingleColourLookup(new SourceBlock(1, 0, 0), new SourceBlock(0, 2, 0)),
            new SingleColourLookup(new SourceBlock(1, 0, 1), new SourceBlock(0, 2, 1)),
            new SingleColourLookup(new SourceBlock(1, 0, 2), new SourceBlock(0, 2, 2)),
            new SingleColourLookup(new SourceBlock(1, 0, 3), new SourceBlock(0, 3, 1)),
            new SingleColourLookup(new SourceBlock(1, 0, 4), new SourceBlock(0, 3, 0)),
            new SingleColourLookup(new SourceBlock(2, 0, 3), new SourceBlock(0, 3, 1)),
            new SingleColourLookup(new SourceBlock(2, 0, 2), new SourceBlock(0, 3, 2)),
            new SingleColourLookup(new SourceBlock(2, 0, 1), new SourceBlock(0, 4, 1)),
            new SingleColourLookup(new SourceBlock(2, 0, 0), new SourceBlock(0, 4, 0)),
            new SingleColourLookup(new SourceBlock(2, 0, 1), new SourceBlock(0, 4, 1)),
            new SingleColourLookup(new SourceBlock(2, 0, 2), new SourceBlock(0, 4, 2)),
            new SingleColourLookup(new SourceBlock(2, 0, 3), new SourceBlock(0, 5, 1)),
            new SingleColourLookup(new SourceBlock(2, 0, 4), new SourceBlock(0, 5, 0)),
            new SingleColourLookup(new SourceBlock(3, 0, 3), new SourceBlock(0, 5, 1)),
            new SingleColourLookup(new SourceBlock(3, 0, 2), new SourceBlock(0, 5, 2)),
            new SingleColourLookup(new SourceBlock(3, 0, 1), new SourceBlock(0, 6, 1)),
            new SingleColourLookup(new SourceBlock(3, 0, 0), new SourceBlock(0, 6, 0)),
            new SingleColourLookup(new SourceBlock(3, 0, 1), new SourceBlock(0, 6, 1)),
            new SingleColourLookup(new SourceBlock(3, 0, 2), new SourceBlock(0, 6, 2)),
            new SingleColourLookup(new SourceBlock(3, 0, 3), new SourceBlock(0, 7, 1)),
            new SingleColourLookup(new SourceBlock(3, 0, 4), new SourceBlock(0, 7, 0)),
            new SingleColourLookup(new SourceBlock(4, 0, 4), new SourceBlock(0, 7, 1)),
            new SingleColourLookup(new SourceBlock(4, 0, 3), new SourceBlock(0, 7, 2)),
            new SingleColourLookup(new SourceBlock(4, 0, 2), new SourceBlock(1, 7, 1)),
            new SingleColourLookup(new SourceBlock(4, 0, 1), new SourceBlock(1, 7, 0)),
            new SingleColourLookup(new SourceBlock(4, 0, 0), new SourceBlock(0, 8, 0)),
            new SingleColourLookup(new SourceBlock(4, 0, 1), new SourceBlock(0, 8, 1)),
            new SingleColourLookup(new SourceBlock(4, 0, 2), new SourceBlock(2, 7, 1)),
            new SingleColourLookup(new SourceBlock(4, 0, 3), new SourceBlock(2, 7, 0)),
            new SingleColourLookup(new SourceBlock(4, 0, 4), new SourceBlock(0, 9, 0)),
            new SingleColourLookup(new SourceBlock(5, 0, 3), new SourceBlock(0, 9, 1)),
            new SingleColourLookup(new SourceBlock(5, 0, 2), new SourceBlock(3, 7, 1)),
            new SingleColourLookup(new SourceBlock(5, 0, 1), new SourceBlock(3, 7, 0)),
            new SingleColourLookup(new SourceBlock(5, 0, 0), new SourceBlock(0, 10, 0)),
            new SingleColourLookup(new SourceBlock(5, 0, 1), new SourceBlock(0, 10, 1)),
            new SingleColourLookup(new SourceBlock(5, 0, 2), new SourceBlock(0, 10, 2)),
            new SingleColourLookup(new SourceBlock(5, 0, 3), new SourceBlock(0, 11, 1)),
            new SingleColourLookup(new SourceBlock(5, 0, 4), new SourceBlock(0, 11, 0)),
            new SingleColourLookup(new SourceBlock(6, 0, 3), new SourceBlock(0, 11, 1)),
            new SingleColourLookup(new SourceBlock(6, 0, 2), new SourceBlock(0, 11, 2)),
            new SingleColourLookup(new SourceBlock(6, 0, 1), new SourceBlock(0, 12, 1)),
            new SingleColourLookup(new SourceBlock(6, 0, 0), new SourceBlock(0, 12, 0)),
            new SingleColourLookup(new SourceBlock(6, 0, 1), new SourceBlock(0, 12, 1)),
            new SingleColourLookup(new SourceBlock(6, 0, 2), new SourceBlock(0, 12, 2)),
            new SingleColourLookup(new SourceBlock(6, 0, 3), new SourceBlock(0, 13, 1)),
            new SingleColourLookup(new SourceBlock(6, 0, 4), new SourceBlock(0, 13, 0)),
            new SingleColourLookup(new SourceBlock(7, 0, 3), new SourceBlock(0, 13, 1)),
            new SingleColourLookup(new SourceBlock(7, 0, 2), new SourceBlock(0, 13, 2)),
            new SingleColourLookup(new SourceBlock(7, 0, 1), new SourceBlock(0, 14, 1)),
            new SingleColourLookup(new SourceBlock(7, 0, 0), new SourceBlock(0, 14, 0)),
            new SingleColourLookup(new SourceBlock(7, 0, 1), new SourceBlock(0, 14, 1)),
            new SingleColourLookup(new SourceBlock(7, 0, 2), new SourceBlock(0, 14, 2)),
            new SingleColourLookup(new SourceBlock(7, 0, 3), new SourceBlock(0, 15, 1)),
            new SingleColourLookup(new SourceBlock(7, 0, 4), new SourceBlock(0, 15, 0)),
            new SingleColourLookup(new SourceBlock(8, 0, 4), new SourceBlock(0, 15, 1)),
            new SingleColourLookup(new SourceBlock(8, 0, 3), new SourceBlock(0, 15, 2)),
            new SingleColourLookup(new SourceBlock(8, 0, 2), new SourceBlock(1, 15, 1)),
            new SingleColourLookup(new SourceBlock(8, 0, 1), new SourceBlock(1, 15, 0)),
            new SingleColourLookup(new SourceBlock(8, 0, 0), new SourceBlock(0, 16, 0)),
            new SingleColourLookup(new SourceBlock(8, 0, 1), new SourceBlock(0, 16, 1)),
            new SingleColourLookup(new SourceBlock(8, 0, 2), new SourceBlock(2, 15, 1)),
            new SingleColourLookup(new SourceBlock(8, 0, 3), new SourceBlock(2, 15, 0)),
            new SingleColourLookup(new SourceBlock(8, 0, 4), new SourceBlock(0, 17, 0)),
            new SingleColourLookup(new SourceBlock(9, 0, 3), new SourceBlock(0, 17, 1)),
            new SingleColourLookup(new SourceBlock(9, 0, 2), new SourceBlock(3, 15, 1)),
            new SingleColourLookup(new SourceBlock(9, 0, 1), new SourceBlock(3, 15, 0)),
            new SingleColourLookup(new SourceBlock(9, 0, 0), new SourceBlock(0, 18, 0)),
            new SingleColourLookup(new SourceBlock(9, 0, 1), new SourceBlock(0, 18, 1)),
            new SingleColourLookup(new SourceBlock(9, 0, 2), new SourceBlock(0, 18, 2)),
            new SingleColourLookup(new SourceBlock(9, 0, 3), new SourceBlock(0, 19, 1)),
            new SingleColourLookup(new SourceBlock(9, 0, 4), new SourceBlock(0, 19, 0)),
            new SingleColourLookup(new SourceBlock(10, 0, 3), new SourceBlock(0, 19, 1)),
            new SingleColourLookup(new SourceBlock(10, 0, 2), new SourceBlock(0, 19, 2)),
            new SingleColourLookup(new SourceBlock(10, 0, 1), new SourceBlock(0, 20, 1)),
            new SingleColourLookup(new SourceBlock(10, 0, 0), new SourceBlock(0, 20, 0)),
            new SingleColourLookup(new SourceBlock(10, 0, 1), new SourceBlock(0, 20, 1)),
            new SingleColourLookup(new SourceBlock(10, 0, 2), new SourceBlock(0, 20, 2)),
            new SingleColourLookup(new SourceBlock(10, 0, 3), new SourceBlock(0, 21, 1)),
            new SingleColourLookup(new SourceBlock(10, 0, 4), new SourceBlock(0, 21, 0)),
            new SingleColourLookup(new SourceBlock(11, 0, 3), new SourceBlock(0, 21, 1)),
            new SingleColourLookup(new SourceBlock(11, 0, 2), new SourceBlock(0, 21, 2)),
            new SingleColourLookup(new SourceBlock(11, 0, 1), new SourceBlock(0, 22, 1)),
            new SingleColourLookup(new SourceBlock(11, 0, 0), new SourceBlock(0, 22, 0)),
            new SingleColourLookup(new SourceBlock(11, 0, 1), new SourceBlock(0, 22, 1)),
            new SingleColourLookup(new SourceBlock(11, 0, 2), new SourceBlock(0, 22, 2)),
            new SingleColourLookup(new SourceBlock(11, 0, 3), new SourceBlock(0, 23, 1)),
            new SingleColourLookup(new SourceBlock(11, 0, 4), new SourceBlock(0, 23, 0)),
            new SingleColourLookup(new SourceBlock(12, 0, 4), new SourceBlock(0, 23, 1)),
            new SingleColourLookup(new SourceBlock(12, 0, 3), new SourceBlock(0, 23, 2)),
            new SingleColourLookup(new SourceBlock(12, 0, 2), new SourceBlock(1, 23, 1)),
            new SingleColourLookup(new SourceBlock(12, 0, 1), new SourceBlock(1, 23, 0)),
            new SingleColourLookup(new SourceBlock(12, 0, 0), new SourceBlock(0, 24, 0)),
            new SingleColourLookup(new SourceBlock(12, 0, 1), new SourceBlock(0, 24, 1)),
            new SingleColourLookup(new SourceBlock(12, 0, 2), new SourceBlock(2, 23, 1)),
            new SingleColourLookup(new SourceBlock(12, 0, 3), new SourceBlock(2, 23, 0)),
            new SingleColourLookup(new SourceBlock(12, 0, 4), new SourceBlock(0, 25, 0)),
            new SingleColourLookup(new SourceBlock(13, 0, 3), new SourceBlock(0, 25, 1)),
            new SingleColourLookup(new SourceBlock(13, 0, 2), new SourceBlock(3, 23, 1)),
            new SingleColourLookup(new SourceBlock(13, 0, 1), new SourceBlock(3, 23, 0)),
            new SingleColourLookup(new SourceBlock(13, 0, 0), new SourceBlock(0, 26, 0)),
            new SingleColourLookup(new SourceBlock(13, 0, 1), new SourceBlock(0, 26, 1)),
            new SingleColourLookup(new SourceBlock(13, 0, 2), new SourceBlock(0, 26, 2)),
            new SingleColourLookup(new SourceBlock(13, 0, 3), new SourceBlock(0, 27, 1)),
            new SingleColourLookup(new SourceBlock(13, 0, 4), new SourceBlock(0, 27, 0)),
            new SingleColourLookup(new SourceBlock(14, 0, 3), new SourceBlock(0, 27, 1)),
            new SingleColourLookup(new SourceBlock(14, 0, 2), new SourceBlock(0, 27, 2)),
            new SingleColourLookup(new SourceBlock(14, 0, 1), new SourceBlock(0, 28, 1)),
            new SingleColourLookup(new SourceBlock(14, 0, 0), new SourceBlock(0, 28, 0)),
            new SingleColourLookup(new SourceBlock(14, 0, 1), new SourceBlock(0, 28, 1)),
            new SingleColourLookup(new SourceBlock(14, 0, 2), new SourceBlock(0, 28, 2)),
            new SingleColourLookup(new SourceBlock(14, 0, 3), new SourceBlock(0, 29, 1)),
            new SingleColourLookup(new SourceBlock(14, 0, 4), new SourceBlock(0, 29, 0)),
            new SingleColourLookup(new SourceBlock(15, 0, 3), new SourceBlock(0, 29, 1)),
            new SingleColourLookup(new SourceBlock(15, 0, 2), new SourceBlock(0, 29, 2)),
            new SingleColourLookup(new SourceBlock(15, 0, 1), new SourceBlock(0, 30, 1)),
            new SingleColourLookup(new SourceBlock(15, 0, 0), new SourceBlock(0, 30, 0)),
            new SingleColourLookup(new SourceBlock(15, 0, 1), new SourceBlock(0, 30, 1)),
            new SingleColourLookup(new SourceBlock(15, 0, 2), new SourceBlock(0, 30, 2)),
            new SingleColourLookup(new SourceBlock(15, 0, 3), new SourceBlock(0, 31, 1)),
            new SingleColourLookup(new SourceBlock(15, 0, 4), new SourceBlock(0, 31, 0)),
            new SingleColourLookup(new SourceBlock(16, 0, 4), new SourceBlock(0, 31, 1)),
            new SingleColourLookup(new SourceBlock(16, 0, 3), new SourceBlock(0, 31, 2)),
            new SingleColourLookup(new SourceBlock(16, 0, 2), new SourceBlock(1, 31, 1)),
            new SingleColourLookup(new SourceBlock(16, 0, 1), new SourceBlock(1, 31, 0)),
            new SingleColourLookup(new SourceBlock(16, 0, 0), new SourceBlock(4, 28, 0)),
            new SingleColourLookup(new SourceBlock(16, 0, 1), new SourceBlock(4, 28, 1)),
            new SingleColourLookup(new SourceBlock(16, 0, 2), new SourceBlock(2, 31, 1)),
            new SingleColourLookup(new SourceBlock(16, 0, 3), new SourceBlock(2, 31, 0)),
            new SingleColourLookup(new SourceBlock(16, 0, 4), new SourceBlock(4, 29, 0)),
            new SingleColourLookup(new SourceBlock(17, 0, 3), new SourceBlock(4, 29, 1)),
            new SingleColourLookup(new SourceBlock(17, 0, 2), new SourceBlock(3, 31, 1)),
            new SingleColourLookup(new SourceBlock(17, 0, 1), new SourceBlock(3, 31, 0)),
            new SingleColourLookup(new SourceBlock(17, 0, 0), new SourceBlock(4, 30, 0)),
            new SingleColourLookup(new SourceBlock(17, 0, 1), new SourceBlock(4, 30, 1)),
            new SingleColourLookup(new SourceBlock(17, 0, 2), new SourceBlock(4, 30, 2)),
            new SingleColourLookup(new SourceBlock(17, 0, 3), new SourceBlock(4, 31, 1)),
            new SingleColourLookup(new SourceBlock(17, 0, 4), new SourceBlock(4, 31, 0)),
            new SingleColourLookup(new SourceBlock(18, 0, 3), new SourceBlock(4, 31, 1)),
            new SingleColourLookup(new SourceBlock(18, 0, 2), new SourceBlock(4, 31, 2)),
            new SingleColourLookup(new SourceBlock(18, 0, 1), new SourceBlock(5, 31, 1)),
            new SingleColourLookup(new SourceBlock(18, 0, 0), new SourceBlock(5, 31, 0)),
            new SingleColourLookup(new SourceBlock(18, 0, 1), new SourceBlock(5, 31, 1)),
            new SingleColourLookup(new SourceBlock(18, 0, 2), new SourceBlock(5, 31, 2)),
            new SingleColourLookup(new SourceBlock(18, 0, 3), new SourceBlock(6, 31, 1)),
            new SingleColourLookup(new SourceBlock(18, 0, 4), new SourceBlock(6, 31, 0)),
            new SingleColourLookup(new SourceBlock(19, 0, 3), new SourceBlock(6, 31, 1)),
            new SingleColourLookup(new SourceBlock(19, 0, 2), new SourceBlock(6, 31, 2)),
            new SingleColourLookup(new SourceBlock(19, 0, 1), new SourceBlock(7, 31, 1)),
            new SingleColourLookup(new SourceBlock(19, 0, 0), new SourceBlock(7, 31, 0)),
            new SingleColourLookup(new SourceBlock(19, 0, 1), new SourceBlock(7, 31, 1)),
            new SingleColourLookup(new SourceBlock(19, 0, 2), new SourceBlock(7, 31, 2)),
            new SingleColourLookup(new SourceBlock(19, 0, 3), new SourceBlock(8, 31, 1)),
            new SingleColourLookup(new SourceBlock(19, 0, 4), new SourceBlock(8, 31, 0)),
            new SingleColourLookup(new SourceBlock(20, 0, 4), new SourceBlock(8, 31, 1)),
            new SingleColourLookup(new SourceBlock(20, 0, 3), new SourceBlock(8, 31, 2)),
            new SingleColourLookup(new SourceBlock(20, 0, 2), new SourceBlock(9, 31, 1)),
            new SingleColourLookup(new SourceBlock(20, 0, 1), new SourceBlock(9, 31, 0)),
            new SingleColourLookup(new SourceBlock(20, 0, 0), new SourceBlock(12, 28, 0)),
            new SingleColourLookup(new SourceBlock(20, 0, 1), new SourceBlock(12, 28, 1)),
            new SingleColourLookup(new SourceBlock(20, 0, 2), new SourceBlock(10, 31, 1)),
            new SingleColourLookup(new SourceBlock(20, 0, 3), new SourceBlock(10, 31, 0)),
            new SingleColourLookup(new SourceBlock(20, 0, 4), new SourceBlock(12, 29, 0)),
            new SingleColourLookup(new SourceBlock(21, 0, 3), new SourceBlock(12, 29, 1)),
            new SingleColourLookup(new SourceBlock(21, 0, 2), new SourceBlock(11, 31, 1)),
            new SingleColourLookup(new SourceBlock(21, 0, 1), new SourceBlock(11, 31, 0)),
            new SingleColourLookup(new SourceBlock(21, 0, 0), new SourceBlock(12, 30, 0)),
            new SingleColourLookup(new SourceBlock(21, 0, 1), new SourceBlock(12, 30, 1)),
            new SingleColourLookup(new SourceBlock(21, 0, 2), new SourceBlock(12, 30, 2)),
            new SingleColourLookup(new SourceBlock(21, 0, 3), new SourceBlock(12, 31, 1)),
            new SingleColourLookup(new SourceBlock(21, 0, 4), new SourceBlock(12, 31, 0)),
            new SingleColourLookup(new SourceBlock(22, 0, 3), new SourceBlock(12, 31, 1)),
            new SingleColourLookup(new SourceBlock(22, 0, 2), new SourceBlock(12, 31, 2)),
            new SingleColourLookup(new SourceBlock(22, 0, 1), new SourceBlock(13, 31, 1)),
            new SingleColourLookup(new SourceBlock(22, 0, 0), new SourceBlock(13, 31, 0)),
            new SingleColourLookup(new SourceBlock(22, 0, 1), new SourceBlock(13, 31, 1)),
            new SingleColourLookup(new SourceBlock(22, 0, 2), new SourceBlock(13, 31, 2)),
            new SingleColourLookup(new SourceBlock(22, 0, 3), new SourceBlock(14, 31, 1)),
            new SingleColourLookup(new SourceBlock(22, 0, 4), new SourceBlock(14, 31, 0)),
            new SingleColourLookup(new SourceBlock(23, 0, 3), new SourceBlock(14, 31, 1)),
            new SingleColourLookup(new SourceBlock(23, 0, 2), new SourceBlock(14, 31, 2)),
            new SingleColourLookup(new SourceBlock(23, 0, 1), new SourceBlock(15, 31, 1)),
            new SingleColourLookup(new SourceBlock(23, 0, 0), new SourceBlock(15, 31, 0)),
            new SingleColourLookup(new SourceBlock(23, 0, 1), new SourceBlock(15, 31, 1)),
            new SingleColourLookup(new SourceBlock(23, 0, 2), new SourceBlock(15, 31, 2)),
            new SingleColourLookup(new SourceBlock(23, 0, 3), new SourceBlock(16, 31, 1)),
            new SingleColourLookup(new SourceBlock(23, 0, 4), new SourceBlock(16, 31, 0)),
            new SingleColourLookup(new SourceBlock(24, 0, 4), new SourceBlock(16, 31, 1)),
            new SingleColourLookup(new SourceBlock(24, 0, 3), new SourceBlock(16, 31, 2)),
            new SingleColourLookup(new SourceBlock(24, 0, 2), new SourceBlock(17, 31, 1)),
            new SingleColourLookup(new SourceBlock(24, 0, 1), new SourceBlock(17, 31, 0)),
            new SingleColourLookup(new SourceBlock(24, 0, 0), new SourceBlock(20, 28, 0)),
            new SingleColourLookup(new SourceBlock(24, 0, 1), new SourceBlock(20, 28, 1)),
            new SingleColourLookup(new SourceBlock(24, 0, 2), new SourceBlock(18, 31, 1)),
            new SingleColourLookup(new SourceBlock(24, 0, 3), new SourceBlock(18, 31, 0)),
            new SingleColourLookup(new SourceBlock(24, 0, 4), new SourceBlock(20, 29, 0)),
            new SingleColourLookup(new SourceBlock(25, 0, 3), new SourceBlock(20, 29, 1)),
            new SingleColourLookup(new SourceBlock(25, 0, 2), new SourceBlock(19, 31, 1)),
            new SingleColourLookup(new SourceBlock(25, 0, 1), new SourceBlock(19, 31, 0)),
            new SingleColourLookup(new SourceBlock(25, 0, 0), new SourceBlock(20, 30, 0)),
            new SingleColourLookup(new SourceBlock(25, 0, 1), new SourceBlock(20, 30, 1)),
            new SingleColourLookup(new SourceBlock(25, 0, 2), new SourceBlock(20, 30, 2)),
            new SingleColourLookup(new SourceBlock(25, 0, 3), new SourceBlock(20, 31, 1)),
            new SingleColourLookup(new SourceBlock(25, 0, 4), new SourceBlock(20, 31, 0)),
            new SingleColourLookup(new SourceBlock(26, 0, 3), new SourceBlock(20, 31, 1)),
            new SingleColourLookup(new SourceBlock(26, 0, 2), new SourceBlock(20, 31, 2)),
            new SingleColourLookup(new SourceBlock(26, 0, 1), new SourceBlock(21, 31, 1)),
            new SingleColourLookup(new SourceBlock(26, 0, 0), new SourceBlock(21, 31, 0)),
            new SingleColourLookup(new SourceBlock(26, 0, 1), new SourceBlock(21, 31, 1)),
            new SingleColourLookup(new SourceBlock(26, 0, 2), new SourceBlock(21, 31, 2)),
            new SingleColourLookup(new SourceBlock(26, 0, 3), new SourceBlock(22, 31, 1)),
            new SingleColourLookup(new SourceBlock(26, 0, 4), new SourceBlock(22, 31, 0)),
            new SingleColourLookup(new SourceBlock(27, 0, 3), new SourceBlock(22, 31, 1)),
            new SingleColourLookup(new SourceBlock(27, 0, 2), new SourceBlock(22, 31, 2)),
            new SingleColourLookup(new SourceBlock(27, 0, 1), new SourceBlock(23, 31, 1)),
            new SingleColourLookup(new SourceBlock(27, 0, 0), new SourceBlock(23, 31, 0)),
            new SingleColourLookup(new SourceBlock(27, 0, 1), new SourceBlock(23, 31, 1)),
            new SingleColourLookup(new SourceBlock(27, 0, 2), new SourceBlock(23, 31, 2)),
            new SingleColourLookup(new SourceBlock(27, 0, 3), new SourceBlock(24, 31, 1)),
            new SingleColourLookup(new SourceBlock(27, 0, 4), new SourceBlock(24, 31, 0)),
            new SingleColourLookup(new SourceBlock(28, 0, 4), new SourceBlock(24, 31, 1)),
            new SingleColourLookup(new SourceBlock(28, 0, 3), new SourceBlock(24, 31, 2)),
            new SingleColourLookup(new SourceBlock(28, 0, 2), new SourceBlock(25, 31, 1)),
            new SingleColourLookup(new SourceBlock(28, 0, 1), new SourceBlock(25, 31, 0)),
            new SingleColourLookup(new SourceBlock(28, 0, 0), new SourceBlock(28, 28, 0)),
            new SingleColourLookup(new SourceBlock(28, 0, 1), new SourceBlock(28, 28, 1)),
            new SingleColourLookup(new SourceBlock(28, 0, 2), new SourceBlock(26, 31, 1)),
            new SingleColourLookup(new SourceBlock(28, 0, 3), new SourceBlock(26, 31, 0)),
            new SingleColourLookup(new SourceBlock(28, 0, 4), new SourceBlock(28, 29, 0)),
            new SingleColourLookup(new SourceBlock(29, 0, 3), new SourceBlock(28, 29, 1)),
            new SingleColourLookup(new SourceBlock(29, 0, 2), new SourceBlock(27, 31, 1)),
            new SingleColourLookup(new SourceBlock(29, 0, 1), new SourceBlock(27, 31, 0)),
            new SingleColourLookup(new SourceBlock(29, 0, 0), new SourceBlock(28, 30, 0)),
            new SingleColourLookup(new SourceBlock(29, 0, 1), new SourceBlock(28, 30, 1)),
            new SingleColourLookup(new SourceBlock(29, 0, 2), new SourceBlock(28, 30, 2)),
            new SingleColourLookup(new SourceBlock(29, 0, 3), new SourceBlock(28, 31, 1)),
            new SingleColourLookup(new SourceBlock(29, 0, 4), new SourceBlock(28, 31, 0)),
            new SingleColourLookup(new SourceBlock(30, 0, 3), new SourceBlock(28, 31, 1)),
            new SingleColourLookup(new SourceBlock(30, 0, 2), new SourceBlock(28, 31, 2)),
            new SingleColourLookup(new SourceBlock(30, 0, 1), new SourceBlock(29, 31, 1)),
            new SingleColourLookup(new SourceBlock(30, 0, 0), new SourceBlock(29, 31, 0)),
            new SingleColourLookup(new SourceBlock(30, 0, 1), new SourceBlock(29, 31, 1)),
            new SingleColourLookup(new SourceBlock(30, 0, 2), new SourceBlock(29, 31, 2)),
            new SingleColourLookup(new SourceBlock(30, 0, 3), new SourceBlock(30, 31, 1)),
            new SingleColourLookup(new SourceBlock(30, 0, 4), new SourceBlock(30, 31, 0)),
            new SingleColourLookup(new SourceBlock(31, 0, 3), new SourceBlock(30, 31, 1)),
            new SingleColourLookup(new SourceBlock(31, 0, 2), new SourceBlock(30, 31, 2)),
            new SingleColourLookup(new SourceBlock(31, 0, 1), new SourceBlock(31, 31, 1)),
            new SingleColourLookup(new SourceBlock(31, 0, 0), new SourceBlock(31, 31, 0))
        };

        public static SingleColourLookup[] Lookup63 = new SingleColourLookup[]
        {
            new SingleColourLookup(new SourceBlock(0, 0, 0), new SourceBlock(0, 0, 0)),
            new SingleColourLookup(new SourceBlock(0, 0, 1), new SourceBlock(0, 1, 1)),
            new SingleColourLookup(new SourceBlock(0, 0, 2), new SourceBlock(0, 1, 0)),
            new SingleColourLookup(new SourceBlock(1, 0, 1), new SourceBlock(0, 2, 1)),
            new SingleColourLookup(new SourceBlock(1, 0, 0), new SourceBlock(0, 2, 0)),
            new SingleColourLookup(new SourceBlock(1, 0, 1), new SourceBlock(0, 3, 1)),
            new SingleColourLookup(new SourceBlock(1, 0, 2), new SourceBlock(0, 3, 0)),
            new SingleColourLookup(new SourceBlock(2, 0, 1), new SourceBlock(0, 4, 1)),
            new SingleColourLookup(new SourceBlock(2, 0, 0), new SourceBlock(0, 4, 0)),
            new SingleColourLookup(new SourceBlock(2, 0, 1), new SourceBlock(0, 5, 1)),
            new SingleColourLookup(new SourceBlock(2, 0, 2), new SourceBlock(0, 5, 0)),
            new SingleColourLookup(new SourceBlock(3, 0, 1), new SourceBlock(0, 6, 1)),
            new SingleColourLookup(new SourceBlock(3, 0, 0), new SourceBlock(0, 6, 0)),
            new SingleColourLookup(new SourceBlock(3, 0, 1), new SourceBlock(0, 7, 1)),
            new SingleColourLookup(new SourceBlock(3, 0, 2), new SourceBlock(0, 7, 0)),
            new SingleColourLookup(new SourceBlock(4, 0, 1), new SourceBlock(0, 8, 1)),
            new SingleColourLookup(new SourceBlock(4, 0, 0), new SourceBlock(0, 8, 0)),
            new SingleColourLookup(new SourceBlock(4, 0, 1), new SourceBlock(0, 9, 1)),
            new SingleColourLookup(new SourceBlock(4, 0, 2), new SourceBlock(0, 9, 0)),
            new SingleColourLookup(new SourceBlock(5, 0, 1), new SourceBlock(0, 10, 1)),
            new SingleColourLookup(new SourceBlock(5, 0, 0), new SourceBlock(0, 10, 0)),
            new SingleColourLookup(new SourceBlock(5, 0, 1), new SourceBlock(0, 11, 1)),
            new SingleColourLookup(new SourceBlock(5, 0, 2), new SourceBlock(0, 11, 0)),
            new SingleColourLookup(new SourceBlock(6, 0, 1), new SourceBlock(0, 12, 1)),
            new SingleColourLookup(new SourceBlock(6, 0, 0), new SourceBlock(0, 12, 0)),
            new SingleColourLookup(new SourceBlock(6, 0, 1), new SourceBlock(0, 13, 1)),
            new SingleColourLookup(new SourceBlock(6, 0, 2), new SourceBlock(0, 13, 0)),
            new SingleColourLookup(new SourceBlock(7, 0, 1), new SourceBlock(0, 14, 1)),
            new SingleColourLookup(new SourceBlock(7, 0, 0), new SourceBlock(0, 14, 0)),
            new SingleColourLookup(new SourceBlock(7, 0, 1), new SourceBlock(0, 15, 1)),
            new SingleColourLookup(new SourceBlock(7, 0, 2), new SourceBlock(0, 15, 0)),
            new SingleColourLookup(new SourceBlock(8, 0, 1), new SourceBlock(0, 16, 1)),
            new SingleColourLookup(new SourceBlock(8, 0, 0), new SourceBlock(0, 16, 0)),
            new SingleColourLookup(new SourceBlock(8, 0, 1), new SourceBlock(0, 17, 1)),
            new SingleColourLookup(new SourceBlock(8, 0, 2), new SourceBlock(0, 17, 0)),
            new SingleColourLookup(new SourceBlock(9, 0, 1), new SourceBlock(0, 18, 1)),
            new SingleColourLookup(new SourceBlock(9, 0, 0), new SourceBlock(0, 18, 0)),
            new SingleColourLookup(new SourceBlock(9, 0, 1), new SourceBlock(0, 19, 1)),
            new SingleColourLookup(new SourceBlock(9, 0, 2), new SourceBlock(0, 19, 0)),
            new SingleColourLookup(new SourceBlock(10, 0, 1), new SourceBlock(0, 20, 1)),
            new SingleColourLookup(new SourceBlock(10, 0, 0), new SourceBlock(0, 20, 0)),
            new SingleColourLookup(new SourceBlock(10, 0, 1), new SourceBlock(0, 21, 1)),
            new SingleColourLookup(new SourceBlock(10, 0, 2), new SourceBlock(0, 21, 0)),
            new SingleColourLookup(new SourceBlock(11, 0, 1), new SourceBlock(0, 22, 1)),
            new SingleColourLookup(new SourceBlock(11, 0, 0), new SourceBlock(0, 22, 0)),
            new SingleColourLookup(new SourceBlock(11, 0, 1), new SourceBlock(0, 23, 1)),
            new SingleColourLookup(new SourceBlock(11, 0, 2), new SourceBlock(0, 23, 0)),
            new SingleColourLookup(new SourceBlock(12, 0, 1), new SourceBlock(0, 24, 1)),
            new SingleColourLookup(new SourceBlock(12, 0, 0), new SourceBlock(0, 24, 0)),
            new SingleColourLookup(new SourceBlock(12, 0, 1), new SourceBlock(0, 25, 1)),
            new SingleColourLookup(new SourceBlock(12, 0, 2), new SourceBlock(0, 25, 0)),
            new SingleColourLookup(new SourceBlock(13, 0, 1), new SourceBlock(0, 26, 1)),
            new SingleColourLookup(new SourceBlock(13, 0, 0), new SourceBlock(0, 26, 0)),
            new SingleColourLookup(new SourceBlock(13, 0, 1), new SourceBlock(0, 27, 1)),
            new SingleColourLookup(new SourceBlock(13, 0, 2), new SourceBlock(0, 27, 0)),
            new SingleColourLookup(new SourceBlock(14, 0, 1), new SourceBlock(0, 28, 1)),
            new SingleColourLookup(new SourceBlock(14, 0, 0), new SourceBlock(0, 28, 0)),
            new SingleColourLookup(new SourceBlock(14, 0, 1), new SourceBlock(0, 29, 1)),
            new SingleColourLookup(new SourceBlock(14, 0, 2), new SourceBlock(0, 29, 0)),
            new SingleColourLookup(new SourceBlock(15, 0, 1), new SourceBlock(0, 30, 1)),
            new SingleColourLookup(new SourceBlock(15, 0, 0), new SourceBlock(0, 30, 0)),
            new SingleColourLookup(new SourceBlock(15, 0, 1), new SourceBlock(0, 31, 1)),
            new SingleColourLookup(new SourceBlock(15, 0, 2), new SourceBlock(0, 31, 0)),
            new SingleColourLookup(new SourceBlock(16, 0, 2), new SourceBlock(1, 31, 1)),
            new SingleColourLookup(new SourceBlock(16, 0, 1), new SourceBlock(1, 31, 0)),
            new SingleColourLookup(new SourceBlock(16, 0, 0), new SourceBlock(0, 32, 0)),
            new SingleColourLookup(new SourceBlock(16, 0, 1), new SourceBlock(2, 31, 0)),
            new SingleColourLookup(new SourceBlock(16, 0, 2), new SourceBlock(0, 33, 0)),
            new SingleColourLookup(new SourceBlock(17, 0, 1), new SourceBlock(3, 31, 0)),
            new SingleColourLookup(new SourceBlock(17, 0, 0), new SourceBlock(0, 34, 0)),
            new SingleColourLookup(new SourceBlock(17, 0, 1), new SourceBlock(4, 31, 0)),
            new SingleColourLookup(new SourceBlock(17, 0, 2), new SourceBlock(0, 35, 0)),
            new SingleColourLookup(new SourceBlock(18, 0, 1), new SourceBlock(5, 31, 0)),
            new SingleColourLookup(new SourceBlock(18, 0, 0), new SourceBlock(0, 36, 0)),
            new SingleColourLookup(new SourceBlock(18, 0, 1), new SourceBlock(6, 31, 0)),
            new SingleColourLookup(new SourceBlock(18, 0, 2), new SourceBlock(0, 37, 0)),
            new SingleColourLookup(new SourceBlock(19, 0, 1), new SourceBlock(7, 31, 0)),
            new SingleColourLookup(new SourceBlock(19, 0, 0), new SourceBlock(0, 38, 0)),
            new SingleColourLookup(new SourceBlock(19, 0, 1), new SourceBlock(8, 31, 0)),
            new SingleColourLookup(new SourceBlock(19, 0, 2), new SourceBlock(0, 39, 0)),
            new SingleColourLookup(new SourceBlock(20, 0, 1), new SourceBlock(9, 31, 0)),
            new SingleColourLookup(new SourceBlock(20, 0, 0), new SourceBlock(0, 40, 0)),
            new SingleColourLookup(new SourceBlock(20, 0, 1), new SourceBlock(10, 31, 0)),
            new SingleColourLookup(new SourceBlock(20, 0, 2), new SourceBlock(0, 41, 0)),
            new SingleColourLookup(new SourceBlock(21, 0, 1), new SourceBlock(11, 31, 0)),
            new SingleColourLookup(new SourceBlock(21, 0, 0), new SourceBlock(0, 42, 0)),
            new SingleColourLookup(new SourceBlock(21, 0, 1), new SourceBlock(12, 31, 0)),
            new SingleColourLookup(new SourceBlock(21, 0, 2), new SourceBlock(0, 43, 0)),
            new SingleColourLookup(new SourceBlock(22, 0, 1), new SourceBlock(13, 31, 0)),
            new SingleColourLookup(new SourceBlock(22, 0, 0), new SourceBlock(0, 44, 0)),
            new SingleColourLookup(new SourceBlock(22, 0, 1), new SourceBlock(14, 31, 0)),
            new SingleColourLookup(new SourceBlock(22, 0, 2), new SourceBlock(0, 45, 0)),
            new SingleColourLookup(new SourceBlock(23, 0, 1), new SourceBlock(15, 31, 0)),
            new SingleColourLookup(new SourceBlock(23, 0, 0), new SourceBlock(0, 46, 0)),
            new SingleColourLookup(new SourceBlock(23, 0, 1), new SourceBlock(0, 47, 1)),
            new SingleColourLookup(new SourceBlock(23, 0, 2), new SourceBlock(0, 47, 0)),
            new SingleColourLookup(new SourceBlock(24, 0, 1), new SourceBlock(0, 48, 1)),
            new SingleColourLookup(new SourceBlock(24, 0, 0), new SourceBlock(0, 48, 0)),
            new SingleColourLookup(new SourceBlock(24, 0, 1), new SourceBlock(0, 49, 1)),
            new SingleColourLookup(new SourceBlock(24, 0, 2), new SourceBlock(0, 49, 0)),
            new SingleColourLookup(new SourceBlock(25, 0, 1), new SourceBlock(0, 50, 1)),
            new SingleColourLookup(new SourceBlock(25, 0, 0), new SourceBlock(0, 50, 0)),
            new SingleColourLookup(new SourceBlock(25, 0, 1), new SourceBlock(0, 51, 1)),
            new SingleColourLookup(new SourceBlock(25, 0, 2), new SourceBlock(0, 51, 0)),
            new SingleColourLookup(new SourceBlock(26, 0, 1), new SourceBlock(0, 52, 1)),
            new SingleColourLookup(new SourceBlock(26, 0, 0), new SourceBlock(0, 52, 0)),
            new SingleColourLookup(new SourceBlock(26, 0, 1), new SourceBlock(0, 53, 1)),
            new SingleColourLookup(new SourceBlock(26, 0, 2), new SourceBlock(0, 53, 0)),
            new SingleColourLookup(new SourceBlock(27, 0, 1), new SourceBlock(0, 54, 1)),
            new SingleColourLookup(new SourceBlock(27, 0, 0), new SourceBlock(0, 54, 0)),
            new SingleColourLookup(new SourceBlock(27, 0, 1), new SourceBlock(0, 55, 1)),
            new SingleColourLookup(new SourceBlock(27, 0, 2), new SourceBlock(0, 55, 0)),
            new SingleColourLookup(new SourceBlock(28, 0, 1), new SourceBlock(0, 56, 1)),
            new SingleColourLookup(new SourceBlock(28, 0, 0), new SourceBlock(0, 56, 0)),
            new SingleColourLookup(new SourceBlock(28, 0, 1), new SourceBlock(0, 57, 1)),
            new SingleColourLookup(new SourceBlock(28, 0, 2), new SourceBlock(0, 57, 0)),
            new SingleColourLookup(new SourceBlock(29, 0, 1), new SourceBlock(0, 58, 1)),
            new SingleColourLookup(new SourceBlock(29, 0, 0), new SourceBlock(0, 58, 0)),
            new SingleColourLookup(new SourceBlock(29, 0, 1), new SourceBlock(0, 59, 1)),
            new SingleColourLookup(new SourceBlock(29, 0, 2), new SourceBlock(0, 59, 0)),
            new SingleColourLookup(new SourceBlock(30, 0, 1), new SourceBlock(0, 60, 1)),
            new SingleColourLookup(new SourceBlock(30, 0, 0), new SourceBlock(0, 60, 0)),
            new SingleColourLookup(new SourceBlock(30, 0, 1), new SourceBlock(0, 61, 1)),
            new SingleColourLookup(new SourceBlock(30, 0, 2), new SourceBlock(0, 61, 0)),
            new SingleColourLookup(new SourceBlock(31, 0, 1), new SourceBlock(0, 62, 1)),
            new SingleColourLookup(new SourceBlock(31, 0, 0), new SourceBlock(0, 62, 0)),
            new SingleColourLookup(new SourceBlock(31, 0, 1), new SourceBlock(0, 63, 1)),
            new SingleColourLookup(new SourceBlock(31, 0, 2), new SourceBlock(0, 63, 0)),
            new SingleColourLookup(new SourceBlock(32, 0, 2), new SourceBlock(1, 63, 1)),
            new SingleColourLookup(new SourceBlock(32, 0, 1), new SourceBlock(1, 63, 0)),
            new SingleColourLookup(new SourceBlock(32, 0, 0), new SourceBlock(16, 48, 0)),
            new SingleColourLookup(new SourceBlock(32, 0, 1), new SourceBlock(2, 63, 0)),
            new SingleColourLookup(new SourceBlock(32, 0, 2), new SourceBlock(16, 49, 0)),
            new SingleColourLookup(new SourceBlock(33, 0, 1), new SourceBlock(3, 63, 0)),
            new SingleColourLookup(new SourceBlock(33, 0, 0), new SourceBlock(16, 50, 0)),
            new SingleColourLookup(new SourceBlock(33, 0, 1), new SourceBlock(4, 63, 0)),
            new SingleColourLookup(new SourceBlock(33, 0, 2), new SourceBlock(16, 51, 0)),
            new SingleColourLookup(new SourceBlock(34, 0, 1), new SourceBlock(5, 63, 0)),
            new SingleColourLookup(new SourceBlock(34, 0, 0), new SourceBlock(16, 52, 0)),
            new SingleColourLookup(new SourceBlock(34, 0, 1), new SourceBlock(6, 63, 0)),
            new SingleColourLookup(new SourceBlock(34, 0, 2), new SourceBlock(16, 53, 0)),
            new SingleColourLookup(new SourceBlock(35, 0, 1), new SourceBlock(7, 63, 0)),
            new SingleColourLookup(new SourceBlock(35, 0, 0), new SourceBlock(16, 54, 0)),
            new SingleColourLookup(new SourceBlock(35, 0, 1), new SourceBlock(8, 63, 0)),
            new SingleColourLookup(new SourceBlock(35, 0, 2), new SourceBlock(16, 55, 0)),
            new SingleColourLookup(new SourceBlock(36, 0, 1), new SourceBlock(9, 63, 0)),
            new SingleColourLookup(new SourceBlock(36, 0, 0), new SourceBlock(16, 56, 0)),
            new SingleColourLookup(new SourceBlock(36, 0, 1), new SourceBlock(10, 63, 0)),
            new SingleColourLookup(new SourceBlock(36, 0, 2), new SourceBlock(16, 57, 0)),
            new SingleColourLookup(new SourceBlock(37, 0, 1), new SourceBlock(11, 63, 0)),
            new SingleColourLookup(new SourceBlock(37, 0, 0), new SourceBlock(16, 58, 0)),
            new SingleColourLookup(new SourceBlock(37, 0, 1), new SourceBlock(12, 63, 0)),
            new SingleColourLookup(new SourceBlock(37, 0, 2), new SourceBlock(16, 59, 0)),
            new SingleColourLookup(new SourceBlock(38, 0, 1), new SourceBlock(13, 63, 0)),
            new SingleColourLookup(new SourceBlock(38, 0, 0), new SourceBlock(16, 60, 0)),
            new SingleColourLookup(new SourceBlock(38, 0, 1), new SourceBlock(14, 63, 0)),
            new SingleColourLookup(new SourceBlock(38, 0, 2), new SourceBlock(16, 61, 0)),
            new SingleColourLookup(new SourceBlock(39, 0, 1), new SourceBlock(15, 63, 0)),
            new SingleColourLookup(new SourceBlock(39, 0, 0), new SourceBlock(16, 62, 0)),
            new SingleColourLookup(new SourceBlock(39, 0, 1), new SourceBlock(16, 63, 1)),
            new SingleColourLookup(new SourceBlock(39, 0, 2), new SourceBlock(16, 63, 0)),
            new SingleColourLookup(new SourceBlock(40, 0, 1), new SourceBlock(17, 63, 1)),
            new SingleColourLookup(new SourceBlock(40, 0, 0), new SourceBlock(17, 63, 0)),
            new SingleColourLookup(new SourceBlock(40, 0, 1), new SourceBlock(18, 63, 1)),
            new SingleColourLookup(new SourceBlock(40, 0, 2), new SourceBlock(18, 63, 0)),
            new SingleColourLookup(new SourceBlock(41, 0, 1), new SourceBlock(19, 63, 1)),
            new SingleColourLookup(new SourceBlock(41, 0, 0), new SourceBlock(19, 63, 0)),
            new SingleColourLookup(new SourceBlock(41, 0, 1), new SourceBlock(20, 63, 1)),
            new SingleColourLookup(new SourceBlock(41, 0, 2), new SourceBlock(20, 63, 0)),
            new SingleColourLookup(new SourceBlock(42, 0, 1), new SourceBlock(21, 63, 1)),
            new SingleColourLookup(new SourceBlock(42, 0, 0), new SourceBlock(21, 63, 0)),
            new SingleColourLookup(new SourceBlock(42, 0, 1), new SourceBlock(22, 63, 1)),
            new SingleColourLookup(new SourceBlock(42, 0, 2), new SourceBlock(22, 63, 0)),
            new SingleColourLookup(new SourceBlock(43, 0, 1), new SourceBlock(23, 63, 1)),
            new SingleColourLookup(new SourceBlock(43, 0, 0), new SourceBlock(23, 63, 0)),
            new SingleColourLookup(new SourceBlock(43, 0, 1), new SourceBlock(24, 63, 1)),
            new SingleColourLookup(new SourceBlock(43, 0, 2), new SourceBlock(24, 63, 0)),
            new SingleColourLookup(new SourceBlock(44, 0, 1), new SourceBlock(25, 63, 1)),
            new SingleColourLookup(new SourceBlock(44, 0, 0), new SourceBlock(25, 63, 0)),
            new SingleColourLookup(new SourceBlock(44, 0, 1), new SourceBlock(26, 63, 1)),
            new SingleColourLookup(new SourceBlock(44, 0, 2), new SourceBlock(26, 63, 0)),
            new SingleColourLookup(new SourceBlock(45, 0, 1), new SourceBlock(27, 63, 1)),
            new SingleColourLookup(new SourceBlock(45, 0, 0), new SourceBlock(27, 63, 0)),
            new SingleColourLookup(new SourceBlock(45, 0, 1), new SourceBlock(28, 63, 1)),
            new SingleColourLookup(new SourceBlock(45, 0, 2), new SourceBlock(28, 63, 0)),
            new SingleColourLookup(new SourceBlock(46, 0, 1), new SourceBlock(29, 63, 1)),
            new SingleColourLookup(new SourceBlock(46, 0, 0), new SourceBlock(29, 63, 0)),
            new SingleColourLookup(new SourceBlock(46, 0, 1), new SourceBlock(30, 63, 1)),
            new SingleColourLookup(new SourceBlock(46, 0, 2), new SourceBlock(30, 63, 0)),
            new SingleColourLookup(new SourceBlock(47, 0, 1), new SourceBlock(31, 63, 1)),
            new SingleColourLookup(new SourceBlock(47, 0, 0), new SourceBlock(31, 63, 0)),
            new SingleColourLookup(new SourceBlock(47, 0, 1), new SourceBlock(32, 63, 1)),
            new SingleColourLookup(new SourceBlock(47, 0, 2), new SourceBlock(32, 63, 0)),
            new SingleColourLookup(new SourceBlock(48, 0, 2), new SourceBlock(33, 63, 1)),
            new SingleColourLookup(new SourceBlock(48, 0, 1), new SourceBlock(33, 63, 0)),
            new SingleColourLookup(new SourceBlock(48, 0, 0), new SourceBlock(48, 48, 0)),
            new SingleColourLookup(new SourceBlock(48, 0, 1), new SourceBlock(34, 63, 0)),
            new SingleColourLookup(new SourceBlock(48, 0, 2), new SourceBlock(48, 49, 0)),
            new SingleColourLookup(new SourceBlock(49, 0, 1), new SourceBlock(35, 63, 0)),
            new SingleColourLookup(new SourceBlock(49, 0, 0), new SourceBlock(48, 50, 0)),
            new SingleColourLookup(new SourceBlock(49, 0, 1), new SourceBlock(36, 63, 0)),
            new SingleColourLookup(new SourceBlock(49, 0, 2), new SourceBlock(48, 51, 0)),
            new SingleColourLookup(new SourceBlock(50, 0, 1), new SourceBlock(37, 63, 0)),
            new SingleColourLookup(new SourceBlock(50, 0, 0), new SourceBlock(48, 52, 0)),
            new SingleColourLookup(new SourceBlock(50, 0, 1), new SourceBlock(38, 63, 0)),
            new SingleColourLookup(new SourceBlock(50, 0, 2), new SourceBlock(48, 53, 0)),
            new SingleColourLookup(new SourceBlock(51, 0, 1), new SourceBlock(39, 63, 0)),
            new SingleColourLookup(new SourceBlock(51, 0, 0), new SourceBlock(48, 54, 0)),
            new SingleColourLookup(new SourceBlock(51, 0, 1), new SourceBlock(40, 63, 0)),
            new SingleColourLookup(new SourceBlock(51, 0, 2), new SourceBlock(48, 55, 0)),
            new SingleColourLookup(new SourceBlock(52, 0, 1), new SourceBlock(41, 63, 0)),
            new SingleColourLookup(new SourceBlock(52, 0, 0), new SourceBlock(48, 56, 0)),
            new SingleColourLookup(new SourceBlock(52, 0, 1), new SourceBlock(42, 63, 0)),
            new SingleColourLookup(new SourceBlock(52, 0, 2), new SourceBlock(48, 57, 0)),
            new SingleColourLookup(new SourceBlock(53, 0, 1), new SourceBlock(43, 63, 0)),
            new SingleColourLookup(new SourceBlock(53, 0, 0), new SourceBlock(48, 58, 0)),
            new SingleColourLookup(new SourceBlock(53, 0, 1), new SourceBlock(44, 63, 0)),
            new SingleColourLookup(new SourceBlock(53, 0, 2), new SourceBlock(48, 59, 0)),
            new SingleColourLookup(new SourceBlock(54, 0, 1), new SourceBlock(45, 63, 0)),
            new SingleColourLookup(new SourceBlock(54, 0, 0), new SourceBlock(48, 60, 0)),
            new SingleColourLookup(new SourceBlock(54, 0, 1), new SourceBlock(46, 63, 0)),
            new SingleColourLookup(new SourceBlock(54, 0, 2), new SourceBlock(48, 61, 0)),
            new SingleColourLookup(new SourceBlock(55, 0, 1), new SourceBlock(47, 63, 0)),
            new SingleColourLookup(new SourceBlock(55, 0, 0), new SourceBlock(48, 62, 0)),
            new SingleColourLookup(new SourceBlock(55, 0, 1), new SourceBlock(48, 63, 1)),
            new SingleColourLookup(new SourceBlock(55, 0, 2), new SourceBlock(48, 63, 0)),
            new SingleColourLookup(new SourceBlock(56, 0, 1), new SourceBlock(49, 63, 1)),
            new SingleColourLookup(new SourceBlock(56, 0, 0), new SourceBlock(49, 63, 0)),
            new SingleColourLookup(new SourceBlock(56, 0, 1), new SourceBlock(50, 63, 1)),
            new SingleColourLookup(new SourceBlock(56, 0, 2), new SourceBlock(50, 63, 0)),
            new SingleColourLookup(new SourceBlock(57, 0, 1), new SourceBlock(51, 63, 1)),
            new SingleColourLookup(new SourceBlock(57, 0, 0), new SourceBlock(51, 63, 0)),
            new SingleColourLookup(new SourceBlock(57, 0, 1), new SourceBlock(52, 63, 1)),
            new SingleColourLookup(new SourceBlock(57, 0, 2), new SourceBlock(52, 63, 0)),
            new SingleColourLookup(new SourceBlock(58, 0, 1), new SourceBlock(53, 63, 1)),
            new SingleColourLookup(new SourceBlock(58, 0, 0), new SourceBlock(53, 63, 0)),
            new SingleColourLookup(new SourceBlock(58, 0, 1), new SourceBlock(54, 63, 1)),
            new SingleColourLookup(new SourceBlock(58, 0, 2), new SourceBlock(54, 63, 0)),
            new SingleColourLookup(new SourceBlock(59, 0, 1), new SourceBlock(55, 63, 1)),
            new SingleColourLookup(new SourceBlock(59, 0, 0), new SourceBlock(55, 63, 0)),
            new SingleColourLookup(new SourceBlock(59, 0, 1), new SourceBlock(56, 63, 1)),
            new SingleColourLookup(new SourceBlock(59, 0, 2), new SourceBlock(56, 63, 0)),
            new SingleColourLookup(new SourceBlock(60, 0, 1), new SourceBlock(57, 63, 1)),
            new SingleColourLookup(new SourceBlock(60, 0, 0), new SourceBlock(57, 63, 0)),
            new SingleColourLookup(new SourceBlock(60, 0, 1), new SourceBlock(58, 63, 1)),
            new SingleColourLookup(new SourceBlock(60, 0, 2), new SourceBlock(58, 63, 0)),
            new SingleColourLookup(new SourceBlock(61, 0, 1), new SourceBlock(59, 63, 1)),
            new SingleColourLookup(new SourceBlock(61, 0, 0), new SourceBlock(59, 63, 0)),
            new SingleColourLookup(new SourceBlock(61, 0, 1), new SourceBlock(60, 63, 1)),
            new SingleColourLookup(new SourceBlock(61, 0, 2), new SourceBlock(60, 63, 0)),
            new SingleColourLookup(new SourceBlock(62, 0, 1), new SourceBlock(61, 63, 1)),
            new SingleColourLookup(new SourceBlock(62, 0, 0), new SourceBlock(61, 63, 0)),
            new SingleColourLookup(new SourceBlock(62, 0, 1), new SourceBlock(62, 63, 1)),
            new SingleColourLookup(new SourceBlock(62, 0, 2), new SourceBlock(62, 63, 0)),
            new SingleColourLookup(new SourceBlock(63, 0, 1), new SourceBlock(63, 63, 1)),
            new SingleColourLookup(new SourceBlock(63, 0, 0), new SourceBlock(63, 63, 0))
        };

        public static SingleColourLookup[] Lookup54 = new SingleColourLookup[]
        {
            new SingleColourLookup(new SourceBlock(0, 0, 0), new SourceBlock(0, 0, 0)),
            new SingleColourLookup(new SourceBlock(0, 0, 1), new SourceBlock(0, 1, 1)),
            new SingleColourLookup(new SourceBlock(0, 0, 2), new SourceBlock(0, 1, 0)),
            new SingleColourLookup(new SourceBlock(0, 0, 3), new SourceBlock(0, 1, 1)),
            new SingleColourLookup(new SourceBlock(0, 0, 4), new SourceBlock(0, 2, 1)),
            new SingleColourLookup(new SourceBlock(1, 0, 3), new SourceBlock(0, 2, 0)),
            new SingleColourLookup(new SourceBlock(1, 0, 2), new SourceBlock(0, 2, 1)),
            new SingleColourLookup(new SourceBlock(1, 0, 1), new SourceBlock(0, 3, 1)),
            new SingleColourLookup(new SourceBlock(1, 0, 0), new SourceBlock(0, 3, 0)),
            new SingleColourLookup(new SourceBlock(1, 0, 1), new SourceBlock(1, 2, 1)),
            new SingleColourLookup(new SourceBlock(1, 0, 2), new SourceBlock(1, 2, 0)),
            new SingleColourLookup(new SourceBlock(1, 0, 3), new SourceBlock(0, 4, 0)),
            new SingleColourLookup(new SourceBlock(1, 0, 4), new SourceBlock(0, 5, 1)),
            new SingleColourLookup(new SourceBlock(2, 0, 3), new SourceBlock(0, 5, 0)),
            new SingleColourLookup(new SourceBlock(2, 0, 2), new SourceBlock(0, 5, 1)),
            new SingleColourLookup(new SourceBlock(2, 0, 1), new SourceBlock(0, 6, 1)),
            new SingleColourLookup(new SourceBlock(2, 0, 0), new SourceBlock(0, 6, 0)),
            new SingleColourLookup(new SourceBlock(2, 0, 1), new SourceBlock(2, 3, 1)),
            new SingleColourLookup(new SourceBlock(2, 0, 2), new SourceBlock(2, 3, 0)),
            new SingleColourLookup(new SourceBlock(2, 0, 3), new SourceBlock(0, 7, 0)),
            new SingleColourLookup(new SourceBlock(2, 0, 4), new SourceBlock(1, 6, 1)),
            new SingleColourLookup(new SourceBlock(3, 0, 3), new SourceBlock(1, 6, 0)),
            new SingleColourLookup(new SourceBlock(3, 0, 2), new SourceBlock(0, 8, 0)),
            new SingleColourLookup(new SourceBlock(3, 0, 1), new SourceBlock(0, 9, 1)),
            new SingleColourLookup(new SourceBlock(3, 0, 0), new SourceBlock(0, 9, 0)),
            new SingleColourLookup(new SourceBlock(3, 0, 1), new SourceBlock(0, 9, 1)),
            new SingleColourLookup(new SourceBlock(3, 0, 2), new SourceBlock(0, 10, 1)),
            new SingleColourLookup(new SourceBlock(3, 0, 3), new SourceBlock(0, 10, 0)),
            new SingleColourLookup(new SourceBlock(3, 0, 4), new SourceBlock(2, 7, 1)),
            new SingleColourLookup(new SourceBlock(4, 0, 4), new SourceBlock(2, 7, 0)),
            new SingleColourLookup(new SourceBlock(4, 0, 3), new SourceBlock(0, 11, 0)),
            new SingleColourLookup(new SourceBlock(4, 0, 2), new SourceBlock(1, 10, 1)),
            new SingleColourLookup(new SourceBlock(4, 0, 1), new SourceBlock(1, 10, 0)),
            new SingleColourLookup(new SourceBlock(4, 0, 0), new SourceBlock(0, 12, 0)),
            new SingleColourLookup(new SourceBlock(4, 0, 1), new SourceBlock(0, 13, 1)),
            new SingleColourLookup(new SourceBlock(4, 0, 2), new SourceBlock(0, 13, 0)),
            new SingleColourLookup(new SourceBlock(4, 0, 3), new SourceBlock(0, 13, 1)),
            new SingleColourLookup(new SourceBlock(4, 0, 4), new SourceBlock(0, 14, 1)),
            new SingleColourLookup(new SourceBlock(5, 0, 3), new SourceBlock(0, 14, 0)),
            new SingleColourLookup(new SourceBlock(5, 0, 2), new SourceBlock(2, 11, 1)),
            new SingleColourLookup(new SourceBlock(5, 0, 1), new SourceBlock(2, 11, 0)),
            new SingleColourLookup(new SourceBlock(5, 0, 0), new SourceBlock(0, 15, 0)),
            new SingleColourLookup(new SourceBlock(5, 0, 1), new SourceBlock(1, 14, 1)),
            new SingleColourLookup(new SourceBlock(5, 0, 2), new SourceBlock(1, 14, 0)),
            new SingleColourLookup(new SourceBlock(5, 0, 3), new SourceBlock(0, 16, 0)),
            new SingleColourLookup(new SourceBlock(5, 0, 4), new SourceBlock(0, 17, 1)),
            new SingleColourLookup(new SourceBlock(6, 0, 3), new SourceBlock(0, 17, 0)),
            new SingleColourLookup(new SourceBlock(6, 0, 2), new SourceBlock(0, 17, 1)),
            new SingleColourLookup(new SourceBlock(6, 0, 1), new SourceBlock(0, 18, 1)),
            new SingleColourLookup(new SourceBlock(6, 0, 0), new SourceBlock(0, 18, 0)),
            new SingleColourLookup(new SourceBlock(6, 0, 1), new SourceBlock(2, 15, 1)),
            new SingleColourLookup(new SourceBlock(6, 0, 2), new SourceBlock(2, 15, 0)),
            new SingleColourLookup(new SourceBlock(6, 0, 3), new SourceBlock(0, 19, 0)),
            new SingleColourLookup(new SourceBlock(6, 0, 4), new SourceBlock(1, 18, 1)),
            new SingleColourLookup(new SourceBlock(7, 0, 3), new SourceBlock(1, 18, 0)),
            new SingleColourLookup(new SourceBlock(7, 0, 2), new SourceBlock(0, 20, 0)),
            new SingleColourLookup(new SourceBlock(7, 0, 1), new SourceBlock(0, 21, 1)),
            new SingleColourLookup(new SourceBlock(7, 0, 0), new SourceBlock(0, 21, 0)),
            new SingleColourLookup(new SourceBlock(7, 0, 1), new SourceBlock(0, 21, 1)),
            new SingleColourLookup(new SourceBlock(7, 0, 2), new SourceBlock(0, 22, 1)),
            new SingleColourLookup(new SourceBlock(7, 0, 3), new SourceBlock(0, 22, 0)),
            new SingleColourLookup(new SourceBlock(7, 0, 4), new SourceBlock(2, 19, 1)),
            new SingleColourLookup(new SourceBlock(8, 0, 4), new SourceBlock(2, 19, 0)),
            new SingleColourLookup(new SourceBlock(8, 0, 3), new SourceBlock(0, 23, 0)),
            new SingleColourLookup(new SourceBlock(8, 0, 2), new SourceBlock(1, 22, 1)),
            new SingleColourLookup(new SourceBlock(8, 0, 1), new SourceBlock(1, 22, 0)),
            new SingleColourLookup(new SourceBlock(8, 0, 0), new SourceBlock(0, 24, 0)),
            new SingleColourLookup(new SourceBlock(8, 0, 1), new SourceBlock(0, 25, 1)),
            new SingleColourLookup(new SourceBlock(8, 0, 2), new SourceBlock(0, 25, 0)),
            new SingleColourLookup(new SourceBlock(8, 0, 3), new SourceBlock(0, 25, 1)),
            new SingleColourLookup(new SourceBlock(8, 0, 4), new SourceBlock(0, 26, 1)),
            new SingleColourLookup(new SourceBlock(9, 0, 3), new SourceBlock(0, 26, 0)),
            new SingleColourLookup(new SourceBlock(9, 0, 2), new SourceBlock(2, 23, 1)),
            new SingleColourLookup(new SourceBlock(9, 0, 1), new SourceBlock(2, 23, 0)),
            new SingleColourLookup(new SourceBlock(9, 0, 0), new SourceBlock(0, 27, 0)),
            new SingleColourLookup(new SourceBlock(9, 0, 1), new SourceBlock(1, 26, 1)),
            new SingleColourLookup(new SourceBlock(9, 0, 2), new SourceBlock(1, 26, 0)),
            new SingleColourLookup(new SourceBlock(9, 0, 3), new SourceBlock(0, 28, 0)),
            new SingleColourLookup(new SourceBlock(9, 0, 4), new SourceBlock(0, 29, 1)),
            new SingleColourLookup(new SourceBlock(10, 0, 3), new SourceBlock(0, 29, 0)),
            new SingleColourLookup(new SourceBlock(10, 0, 2), new SourceBlock(0, 29, 1)),
            new SingleColourLookup(new SourceBlock(10, 0, 1), new SourceBlock(0, 30, 1)),
            new SingleColourLookup(new SourceBlock(10, 0, 0), new SourceBlock(0, 30, 0)),
            new SingleColourLookup(new SourceBlock(10, 0, 1), new SourceBlock(2, 27, 1)),
            new SingleColourLookup(new SourceBlock(10, 0, 2), new SourceBlock(2, 27, 0)),
            new SingleColourLookup(new SourceBlock(10, 0, 3), new SourceBlock(0, 31, 0)),
            new SingleColourLookup(new SourceBlock(10, 0, 4), new SourceBlock(1, 30, 1)),
            new SingleColourLookup(new SourceBlock(11, 0, 3), new SourceBlock(1, 30, 0)),
            new SingleColourLookup(new SourceBlock(11, 0, 2), new SourceBlock(4, 24, 0)),
            new SingleColourLookup(new SourceBlock(11, 0, 1), new SourceBlock(1, 31, 1)),
            new SingleColourLookup(new SourceBlock(11, 0, 0), new SourceBlock(1, 31, 0)),
            new SingleColourLookup(new SourceBlock(11, 0, 1), new SourceBlock(1, 31, 1)),
            new SingleColourLookup(new SourceBlock(11, 0, 2), new SourceBlock(2, 30, 1)),
            new SingleColourLookup(new SourceBlock(11, 0, 3), new SourceBlock(2, 30, 0)),
            new SingleColourLookup(new SourceBlock(11, 0, 4), new SourceBlock(2, 31, 1)),
            new SingleColourLookup(new SourceBlock(12, 0, 4), new SourceBlock(2, 31, 0)),
            new SingleColourLookup(new SourceBlock(12, 0, 3), new SourceBlock(4, 27, 0)),
            new SingleColourLookup(new SourceBlock(12, 0, 2), new SourceBlock(3, 30, 1)),
            new SingleColourLookup(new SourceBlock(12, 0, 1), new SourceBlock(3, 30, 0)),
            new SingleColourLookup(new SourceBlock(12, 0, 0), new SourceBlock(4, 28, 0)),
            new SingleColourLookup(new SourceBlock(12, 0, 1), new SourceBlock(3, 31, 1)),
            new SingleColourLookup(new SourceBlock(12, 0, 2), new SourceBlock(3, 31, 0)),
            new SingleColourLookup(new SourceBlock(12, 0, 3), new SourceBlock(3, 31, 1)),
            new SingleColourLookup(new SourceBlock(12, 0, 4), new SourceBlock(4, 30, 1)),
            new SingleColourLookup(new SourceBlock(13, 0, 3), new SourceBlock(4, 30, 0)),
            new SingleColourLookup(new SourceBlock(13, 0, 2), new SourceBlock(6, 27, 1)),
            new SingleColourLookup(new SourceBlock(13, 0, 1), new SourceBlock(6, 27, 0)),
            new SingleColourLookup(new SourceBlock(13, 0, 0), new SourceBlock(4, 31, 0)),
            new SingleColourLookup(new SourceBlock(13, 0, 1), new SourceBlock(5, 30, 1)),
            new SingleColourLookup(new SourceBlock(13, 0, 2), new SourceBlock(5, 30, 0)),
            new SingleColourLookup(new SourceBlock(13, 0, 3), new SourceBlock(8, 24, 0)),
            new SingleColourLookup(new SourceBlock(13, 0, 4), new SourceBlock(5, 31, 1)),
            new SingleColourLookup(new SourceBlock(14, 0, 3), new SourceBlock(5, 31, 0)),
            new SingleColourLookup(new SourceBlock(14, 0, 2), new SourceBlock(5, 31, 1)),
            new SingleColourLookup(new SourceBlock(14, 0, 1), new SourceBlock(6, 30, 1)),
            new SingleColourLookup(new SourceBlock(14, 0, 0), new SourceBlock(6, 30, 0)),
            new SingleColourLookup(new SourceBlock(14, 0, 1), new SourceBlock(6, 31, 1)),
            new SingleColourLookup(new SourceBlock(14, 0, 2), new SourceBlock(6, 31, 0)),
            new SingleColourLookup(new SourceBlock(14, 0, 3), new SourceBlock(8, 27, 0)),
            new SingleColourLookup(new SourceBlock(14, 0, 4), new SourceBlock(7, 30, 1)),
            new SingleColourLookup(new SourceBlock(15, 0, 3), new SourceBlock(7, 30, 0)),
            new SingleColourLookup(new SourceBlock(15, 0, 2), new SourceBlock(8, 28, 0)),
            new SingleColourLookup(new SourceBlock(15, 0, 1), new SourceBlock(7, 31, 1)),
            new SingleColourLookup(new SourceBlock(15, 0, 0), new SourceBlock(7, 31, 0)),
            new SingleColourLookup(new SourceBlock(15, 0, 1), new SourceBlock(7, 31, 1)),
            new SingleColourLookup(new SourceBlock(15, 0, 2), new SourceBlock(8, 30, 1)),
            new SingleColourLookup(new SourceBlock(15, 0, 3), new SourceBlock(8, 30, 0)),
            new SingleColourLookup(new SourceBlock(15, 0, 4), new SourceBlock(10, 27, 1)),
            new SingleColourLookup(new SourceBlock(16, 0, 4), new SourceBlock(10, 27, 0)),
            new SingleColourLookup(new SourceBlock(16, 0, 3), new SourceBlock(8, 31, 0)),
            new SingleColourLookup(new SourceBlock(16, 0, 2), new SourceBlock(9, 30, 1)),
            new SingleColourLookup(new SourceBlock(16, 0, 1), new SourceBlock(9, 30, 0)),
            new SingleColourLookup(new SourceBlock(16, 0, 0), new SourceBlock(12, 24, 0)),
            new SingleColourLookup(new SourceBlock(16, 0, 1), new SourceBlock(9, 31, 1)),
            new SingleColourLookup(new SourceBlock(16, 0, 2), new SourceBlock(9, 31, 0)),
            new SingleColourLookup(new SourceBlock(16, 0, 3), new SourceBlock(9, 31, 1)),
            new SingleColourLookup(new SourceBlock(16, 0, 4), new SourceBlock(10, 30, 1)),
            new SingleColourLookup(new SourceBlock(17, 0, 3), new SourceBlock(10, 30, 0)),
            new SingleColourLookup(new SourceBlock(17, 0, 2), new SourceBlock(10, 31, 1)),
            new SingleColourLookup(new SourceBlock(17, 0, 1), new SourceBlock(10, 31, 0)),
            new SingleColourLookup(new SourceBlock(17, 0, 0), new SourceBlock(12, 27, 0)),
            new SingleColourLookup(new SourceBlock(17, 0, 1), new SourceBlock(11, 30, 1)),
            new SingleColourLookup(new SourceBlock(17, 0, 2), new SourceBlock(11, 30, 0)),
            new SingleColourLookup(new SourceBlock(17, 0, 3), new SourceBlock(12, 28, 0)),
            new SingleColourLookup(new SourceBlock(17, 0, 4), new SourceBlock(11, 31, 1)),
            new SingleColourLookup(new SourceBlock(18, 0, 3), new SourceBlock(11, 31, 0)),
            new SingleColourLookup(new SourceBlock(18, 0, 2), new SourceBlock(11, 31, 1)),
            new SingleColourLookup(new SourceBlock(18, 0, 1), new SourceBlock(12, 30, 1)),
            new SingleColourLookup(new SourceBlock(18, 0, 0), new SourceBlock(12, 30, 0)),
            new SingleColourLookup(new SourceBlock(18, 0, 1), new SourceBlock(14, 27, 1)),
            new SingleColourLookup(new SourceBlock(18, 0, 2), new SourceBlock(14, 27, 0)),
            new SingleColourLookup(new SourceBlock(18, 0, 3), new SourceBlock(12, 31, 0)),
            new SingleColourLookup(new SourceBlock(18, 0, 4), new SourceBlock(13, 30, 1)),
            new SingleColourLookup(new SourceBlock(19, 0, 3), new SourceBlock(13, 30, 0)),
            new SingleColourLookup(new SourceBlock(19, 0, 2), new SourceBlock(16, 24, 0)),
            new SingleColourLookup(new SourceBlock(19, 0, 1), new SourceBlock(13, 31, 1)),
            new SingleColourLookup(new SourceBlock(19, 0, 0), new SourceBlock(13, 31, 0)),
            new SingleColourLookup(new SourceBlock(19, 0, 1), new SourceBlock(13, 31, 1)),
            new SingleColourLookup(new SourceBlock(19, 0, 2), new SourceBlock(14, 30, 1)),
            new SingleColourLookup(new SourceBlock(19, 0, 3), new SourceBlock(14, 30, 0)),
            new SingleColourLookup(new SourceBlock(19, 0, 4), new SourceBlock(14, 31, 1)),
            new SingleColourLookup(new SourceBlock(20, 0, 4), new SourceBlock(14, 31, 0)),
            new SingleColourLookup(new SourceBlock(20, 0, 3), new SourceBlock(16, 27, 0)),
            new SingleColourLookup(new SourceBlock(20, 0, 2), new SourceBlock(15, 30, 1)),
            new SingleColourLookup(new SourceBlock(20, 0, 1), new SourceBlock(15, 30, 0)),
            new SingleColourLookup(new SourceBlock(20, 0, 0), new SourceBlock(16, 28, 0)),
            new SingleColourLookup(new SourceBlock(20, 0, 1), new SourceBlock(15, 31, 1)),
            new SingleColourLookup(new SourceBlock(20, 0, 2), new SourceBlock(15, 31, 0)),
            new SingleColourLookup(new SourceBlock(20, 0, 3), new SourceBlock(15, 31, 1)),
            new SingleColourLookup(new SourceBlock(20, 0, 4), new SourceBlock(16, 30, 1)),
            new SingleColourLookup(new SourceBlock(21, 0, 3), new SourceBlock(16, 30, 0)),
            new SingleColourLookup(new SourceBlock(21, 0, 2), new SourceBlock(18, 27, 1)),
            new SingleColourLookup(new SourceBlock(21, 0, 1), new SourceBlock(18, 27, 0)),
            new SingleColourLookup(new SourceBlock(21, 0, 0), new SourceBlock(16, 31, 0)),
            new SingleColourLookup(new SourceBlock(21, 0, 1), new SourceBlock(17, 30, 1)),
            new SingleColourLookup(new SourceBlock(21, 0, 2), new SourceBlock(17, 30, 0)),
            new SingleColourLookup(new SourceBlock(21, 0, 3), new SourceBlock(20, 24, 0)),
            new SingleColourLookup(new SourceBlock(21, 0, 4), new SourceBlock(17, 31, 1)),
            new SingleColourLookup(new SourceBlock(22, 0, 3), new SourceBlock(17, 31, 0)),
            new SingleColourLookup(new SourceBlock(22, 0, 2), new SourceBlock(17, 31, 1)),
            new SingleColourLookup(new SourceBlock(22, 0, 1), new SourceBlock(18, 30, 1)),
            new SingleColourLookup(new SourceBlock(22, 0, 0), new SourceBlock(18, 30, 0)),
            new SingleColourLookup(new SourceBlock(22, 0, 1), new SourceBlock(18, 31, 1)),
            new SingleColourLookup(new SourceBlock(22, 0, 2), new SourceBlock(18, 31, 0)),
            new SingleColourLookup(new SourceBlock(22, 0, 3), new SourceBlock(20, 27, 0)),
            new SingleColourLookup(new SourceBlock(22, 0, 4), new SourceBlock(19, 30, 1)),
            new SingleColourLookup(new SourceBlock(23, 0, 3), new SourceBlock(19, 30, 0)),
            new SingleColourLookup(new SourceBlock(23, 0, 2), new SourceBlock(20, 28, 0)),
            new SingleColourLookup(new SourceBlock(23, 0, 1), new SourceBlock(19, 31, 1)),
            new SingleColourLookup(new SourceBlock(23, 0, 0), new SourceBlock(19, 31, 0)),
            new SingleColourLookup(new SourceBlock(23, 0, 1), new SourceBlock(19, 31, 1)),
            new SingleColourLookup(new SourceBlock(23, 0, 2), new SourceBlock(20, 30, 1)),
            new SingleColourLookup(new SourceBlock(23, 0, 3), new SourceBlock(20, 30, 0)),
            new SingleColourLookup(new SourceBlock(23, 0, 4), new SourceBlock(22, 27, 1)),
            new SingleColourLookup(new SourceBlock(24, 0, 4), new SourceBlock(22, 27, 0)),
            new SingleColourLookup(new SourceBlock(24, 0, 3), new SourceBlock(20, 31, 0)),
            new SingleColourLookup(new SourceBlock(24, 0, 2), new SourceBlock(21, 30, 1)),
            new SingleColourLookup(new SourceBlock(24, 0, 1), new SourceBlock(21, 30, 0)),
            new SingleColourLookup(new SourceBlock(24, 0, 0), new SourceBlock(24, 24, 0)),
            new SingleColourLookup(new SourceBlock(24, 0, 1), new SourceBlock(21, 31, 1)),
            new SingleColourLookup(new SourceBlock(24, 0, 2), new SourceBlock(21, 31, 0)),
            new SingleColourLookup(new SourceBlock(24, 0, 3), new SourceBlock(21, 31, 1)),
            new SingleColourLookup(new SourceBlock(24, 0, 4), new SourceBlock(22, 30, 1)),
            new SingleColourLookup(new SourceBlock(25, 0, 3), new SourceBlock(22, 30, 0)),
            new SingleColourLookup(new SourceBlock(25, 0, 2), new SourceBlock(22, 31, 1)),
            new SingleColourLookup(new SourceBlock(25, 0, 1), new SourceBlock(22, 31, 0)),
            new SingleColourLookup(new SourceBlock(25, 0, 0), new SourceBlock(24, 27, 0)),
            new SingleColourLookup(new SourceBlock(25, 0, 1), new SourceBlock(23, 30, 1)),
            new SingleColourLookup(new SourceBlock(25, 0, 2), new SourceBlock(23, 30, 0)),
            new SingleColourLookup(new SourceBlock(25, 0, 3), new SourceBlock(24, 28, 0)),
            new SingleColourLookup(new SourceBlock(25, 0, 4), new SourceBlock(23, 31, 1)),
            new SingleColourLookup(new SourceBlock(26, 0, 3), new SourceBlock(23, 31, 0)),
            new SingleColourLookup(new SourceBlock(26, 0, 2), new SourceBlock(23, 31, 1)),
            new SingleColourLookup(new SourceBlock(26, 0, 1), new SourceBlock(24, 30, 1)),
            new SingleColourLookup(new SourceBlock(26, 0, 0), new SourceBlock(24, 30, 0)),
            new SingleColourLookup(new SourceBlock(26, 0, 1), new SourceBlock(26, 27, 1)),
            new SingleColourLookup(new SourceBlock(26, 0, 2), new SourceBlock(26, 27, 0)),
            new SingleColourLookup(new SourceBlock(26, 0, 3), new SourceBlock(24, 31, 0)),
            new SingleColourLookup(new SourceBlock(26, 0, 4), new SourceBlock(25, 30, 1)),
            new SingleColourLookup(new SourceBlock(27, 0, 3), new SourceBlock(25, 30, 0)),
            new SingleColourLookup(new SourceBlock(27, 0, 2), new SourceBlock(28, 24, 0)),
            new SingleColourLookup(new SourceBlock(27, 0, 1), new SourceBlock(25, 31, 1)),
            new SingleColourLookup(new SourceBlock(27, 0, 0), new SourceBlock(25, 31, 0)),
            new SingleColourLookup(new SourceBlock(27, 0, 1), new SourceBlock(25, 31, 1)),
            new SingleColourLookup(new SourceBlock(27, 0, 2), new SourceBlock(26, 30, 1)),
            new SingleColourLookup(new SourceBlock(27, 0, 3), new SourceBlock(26, 30, 0)),
            new SingleColourLookup(new SourceBlock(27, 0, 4), new SourceBlock(26, 31, 1)),
            new SingleColourLookup(new SourceBlock(28, 0, 4), new SourceBlock(26, 31, 0)),
            new SingleColourLookup(new SourceBlock(28, 0, 3), new SourceBlock(28, 27, 0)),
            new SingleColourLookup(new SourceBlock(28, 0, 2), new SourceBlock(27, 30, 1)),
            new SingleColourLookup(new SourceBlock(28, 0, 1), new SourceBlock(27, 30, 0)),
            new SingleColourLookup(new SourceBlock(28, 0, 0), new SourceBlock(28, 28, 0)),
            new SingleColourLookup(new SourceBlock(28, 0, 1), new SourceBlock(27, 31, 1)),
            new SingleColourLookup(new SourceBlock(28, 0, 2), new SourceBlock(27, 31, 0)),
            new SingleColourLookup(new SourceBlock(28, 0, 3), new SourceBlock(27, 31, 1)),
            new SingleColourLookup(new SourceBlock(28, 0, 4), new SourceBlock(28, 30, 1)),
            new SingleColourLookup(new SourceBlock(29, 0, 3), new SourceBlock(28, 30, 0)),
            new SingleColourLookup(new SourceBlock(29, 0, 2), new SourceBlock(30, 27, 1)),
            new SingleColourLookup(new SourceBlock(29, 0, 1), new SourceBlock(30, 27, 0)),
            new SingleColourLookup(new SourceBlock(29, 0, 0), new SourceBlock(28, 31, 0)),
            new SingleColourLookup(new SourceBlock(29, 0, 1), new SourceBlock(29, 30, 1)),
            new SingleColourLookup(new SourceBlock(29, 0, 2), new SourceBlock(29, 30, 0)),
            new SingleColourLookup(new SourceBlock(29, 0, 3), new SourceBlock(29, 30, 1)),
            new SingleColourLookup(new SourceBlock(29, 0, 4), new SourceBlock(29, 31, 1)),
            new SingleColourLookup(new SourceBlock(30, 0, 3), new SourceBlock(29, 31, 0)),
            new SingleColourLookup(new SourceBlock(30, 0, 2), new SourceBlock(29, 31, 1)),
            new SingleColourLookup(new SourceBlock(30, 0, 1), new SourceBlock(30, 30, 1)),
            new SingleColourLookup(new SourceBlock(30, 0, 0), new SourceBlock(30, 30, 0)),
            new SingleColourLookup(new SourceBlock(30, 0, 1), new SourceBlock(30, 31, 1)),
            new SingleColourLookup(new SourceBlock(30, 0, 2), new SourceBlock(30, 31, 0)),
            new SingleColourLookup(new SourceBlock(30, 0, 3), new SourceBlock(30, 31, 1)),
            new SingleColourLookup(new SourceBlock(30, 0, 4), new SourceBlock(31, 30, 1)),
            new SingleColourLookup(new SourceBlock(31, 0, 3), new SourceBlock(31, 30, 0)),
            new SingleColourLookup(new SourceBlock(31, 0, 2), new SourceBlock(31, 30, 1)),
            new SingleColourLookup(new SourceBlock(31, 0, 1), new SourceBlock(31, 31, 1)),
            new SingleColourLookup(new SourceBlock(31, 0, 0), new SourceBlock(31, 31, 0))
        };

        public static SingleColourLookup[] Lookup64 = new SingleColourLookup[]
        {
            new SingleColourLookup(new SourceBlock(0, 0, 0), new SourceBlock(0, 0, 0)),
            new SingleColourLookup(new SourceBlock(0, 0, 1), new SourceBlock(0, 1, 0)),
            new SingleColourLookup(new SourceBlock(0, 0, 2), new SourceBlock(0, 2, 0)),
            new SingleColourLookup(new SourceBlock(1, 0, 1), new SourceBlock(0, 3, 1)),
            new SingleColourLookup(new SourceBlock(1, 0, 0), new SourceBlock(0, 3, 0)),
            new SingleColourLookup(new SourceBlock(1, 0, 1), new SourceBlock(0, 4, 0)),
            new SingleColourLookup(new SourceBlock(1, 0, 2), new SourceBlock(0, 5, 0)),
            new SingleColourLookup(new SourceBlock(2, 0, 1), new SourceBlock(0, 6, 1)),
            new SingleColourLookup(new SourceBlock(2, 0, 0), new SourceBlock(0, 6, 0)),
            new SingleColourLookup(new SourceBlock(2, 0, 1), new SourceBlock(0, 7, 0)),
            new SingleColourLookup(new SourceBlock(2, 0, 2), new SourceBlock(0, 8, 0)),
            new SingleColourLookup(new SourceBlock(3, 0, 1), new SourceBlock(0, 9, 1)),
            new SingleColourLookup(new SourceBlock(3, 0, 0), new SourceBlock(0, 9, 0)),
            new SingleColourLookup(new SourceBlock(3, 0, 1), new SourceBlock(0, 10, 0)),
            new SingleColourLookup(new SourceBlock(3, 0, 2), new SourceBlock(0, 11, 0)),
            new SingleColourLookup(new SourceBlock(4, 0, 1), new SourceBlock(0, 12, 1)),
            new SingleColourLookup(new SourceBlock(4, 0, 0), new SourceBlock(0, 12, 0)),
            new SingleColourLookup(new SourceBlock(4, 0, 1), new SourceBlock(0, 13, 0)),
            new SingleColourLookup(new SourceBlock(4, 0, 2), new SourceBlock(0, 14, 0)),
            new SingleColourLookup(new SourceBlock(5, 0, 1), new SourceBlock(0, 15, 1)),
            new SingleColourLookup(new SourceBlock(5, 0, 0), new SourceBlock(0, 15, 0)),
            new SingleColourLookup(new SourceBlock(5, 0, 1), new SourceBlock(0, 16, 0)),
            new SingleColourLookup(new SourceBlock(5, 0, 2), new SourceBlock(1, 15, 0)),
            new SingleColourLookup(new SourceBlock(6, 0, 1), new SourceBlock(0, 17, 0)),
            new SingleColourLookup(new SourceBlock(6, 0, 0), new SourceBlock(0, 18, 0)),
            new SingleColourLookup(new SourceBlock(6, 0, 1), new SourceBlock(0, 19, 0)),
            new SingleColourLookup(new SourceBlock(6, 0, 2), new SourceBlock(3, 14, 0)),
            new SingleColourLookup(new SourceBlock(7, 0, 1), new SourceBlock(0, 20, 0)),
            new SingleColourLookup(new SourceBlock(7, 0, 0), new SourceBlock(0, 21, 0)),
            new SingleColourLookup(new SourceBlock(7, 0, 1), new SourceBlock(0, 22, 0)),
            new SingleColourLookup(new SourceBlock(7, 0, 2), new SourceBlock(4, 15, 0)),
            new SingleColourLookup(new SourceBlock(8, 0, 1), new SourceBlock(0, 23, 0)),
            new SingleColourLookup(new SourceBlock(8, 0, 0), new SourceBlock(0, 24, 0)),
            new SingleColourLookup(new SourceBlock(8, 0, 1), new SourceBlock(0, 25, 0)),
            new SingleColourLookup(new SourceBlock(8, 0, 2), new SourceBlock(6, 14, 0)),
            new SingleColourLookup(new SourceBlock(9, 0, 1), new SourceBlock(0, 26, 0)),
            new SingleColourLookup(new SourceBlock(9, 0, 0), new SourceBlock(0, 27, 0)),
            new SingleColourLookup(new SourceBlock(9, 0, 1), new SourceBlock(0, 28, 0)),
            new SingleColourLookup(new SourceBlock(9, 0, 2), new SourceBlock(7, 15, 0)),
            new SingleColourLookup(new SourceBlock(10, 0, 1), new SourceBlock(0, 29, 0)),
            new SingleColourLookup(new SourceBlock(10, 0, 0), new SourceBlock(0, 30, 0)),
            new SingleColourLookup(new SourceBlock(10, 0, 1), new SourceBlock(0, 31, 0)),
            new SingleColourLookup(new SourceBlock(10, 0, 2), new SourceBlock(9, 14, 0)),
            new SingleColourLookup(new SourceBlock(11, 0, 1), new SourceBlock(0, 32, 0)),
            new SingleColourLookup(new SourceBlock(11, 0, 0), new SourceBlock(0, 33, 0)),
            new SingleColourLookup(new SourceBlock(11, 0, 1), new SourceBlock(2, 30, 0)),
            new SingleColourLookup(new SourceBlock(11, 0, 2), new SourceBlock(0, 34, 0)),
            new SingleColourLookup(new SourceBlock(12, 0, 1), new SourceBlock(0, 35, 0)),
            new SingleColourLookup(new SourceBlock(12, 0, 0), new SourceBlock(0, 36, 0)),
            new SingleColourLookup(new SourceBlock(12, 0, 1), new SourceBlock(3, 31, 0)),
            new SingleColourLookup(new SourceBlock(12, 0, 2), new SourceBlock(0, 37, 0)),
            new SingleColourLookup(new SourceBlock(13, 0, 1), new SourceBlock(0, 38, 0)),
            new SingleColourLookup(new SourceBlock(13, 0, 0), new SourceBlock(0, 39, 0)),
            new SingleColourLookup(new SourceBlock(13, 0, 1), new SourceBlock(5, 30, 0)),
            new SingleColourLookup(new SourceBlock(13, 0, 2), new SourceBlock(0, 40, 0)),
            new SingleColourLookup(new SourceBlock(14, 0, 1), new SourceBlock(0, 41, 0)),
            new SingleColourLookup(new SourceBlock(14, 0, 0), new SourceBlock(0, 42, 0)),
            new SingleColourLookup(new SourceBlock(14, 0, 1), new SourceBlock(6, 31, 0)),
            new SingleColourLookup(new SourceBlock(14, 0, 2), new SourceBlock(0, 43, 0)),
            new SingleColourLookup(new SourceBlock(15, 0, 1), new SourceBlock(0, 44, 0)),
            new SingleColourLookup(new SourceBlock(15, 0, 0), new SourceBlock(0, 45, 0)),
            new SingleColourLookup(new SourceBlock(15, 0, 1), new SourceBlock(8, 30, 0)),
            new SingleColourLookup(new SourceBlock(15, 0, 2), new SourceBlock(0, 46, 0)),
            new SingleColourLookup(new SourceBlock(16, 0, 2), new SourceBlock(0, 47, 0)),
            new SingleColourLookup(new SourceBlock(16, 0, 1), new SourceBlock(1, 46, 0)),
            new SingleColourLookup(new SourceBlock(16, 0, 0), new SourceBlock(0, 48, 0)),
            new SingleColourLookup(new SourceBlock(16, 0, 1), new SourceBlock(0, 49, 0)),
            new SingleColourLookup(new SourceBlock(16, 0, 2), new SourceBlock(0, 50, 0)),
            new SingleColourLookup(new SourceBlock(17, 0, 1), new SourceBlock(2, 47, 0)),
            new SingleColourLookup(new SourceBlock(17, 0, 0), new SourceBlock(0, 51, 0)),
            new SingleColourLookup(new SourceBlock(17, 0, 1), new SourceBlock(0, 52, 0)),
            new SingleColourLookup(new SourceBlock(17, 0, 2), new SourceBlock(0, 53, 0)),
            new SingleColourLookup(new SourceBlock(18, 0, 1), new SourceBlock(4, 46, 0)),
            new SingleColourLookup(new SourceBlock(18, 0, 0), new SourceBlock(0, 54, 0)),
            new SingleColourLookup(new SourceBlock(18, 0, 1), new SourceBlock(0, 55, 0)),
            new SingleColourLookup(new SourceBlock(18, 0, 2), new SourceBlock(0, 56, 0)),
            new SingleColourLookup(new SourceBlock(19, 0, 1), new SourceBlock(5, 47, 0)),
            new SingleColourLookup(new SourceBlock(19, 0, 0), new SourceBlock(0, 57, 0)),
            new SingleColourLookup(new SourceBlock(19, 0, 1), new SourceBlock(0, 58, 0)),
            new SingleColourLookup(new SourceBlock(19, 0, 2), new SourceBlock(0, 59, 0)),
            new SingleColourLookup(new SourceBlock(20, 0, 1), new SourceBlock(7, 46, 0)),
            new SingleColourLookup(new SourceBlock(20, 0, 0), new SourceBlock(0, 60, 0)),
            new SingleColourLookup(new SourceBlock(20, 0, 1), new SourceBlock(0, 61, 0)),
            new SingleColourLookup(new SourceBlock(20, 0, 2), new SourceBlock(0, 62, 0)),
            new SingleColourLookup(new SourceBlock(21, 0, 1), new SourceBlock(8, 47, 0)),
            new SingleColourLookup(new SourceBlock(21, 0, 0), new SourceBlock(0, 63, 0)),
            new SingleColourLookup(new SourceBlock(21, 0, 1), new SourceBlock(1, 62, 0)),
            new SingleColourLookup(new SourceBlock(21, 0, 2), new SourceBlock(1, 63, 0)),
            new SingleColourLookup(new SourceBlock(22, 0, 1), new SourceBlock(10, 46, 0)),
            new SingleColourLookup(new SourceBlock(22, 0, 0), new SourceBlock(2, 62, 0)),
            new SingleColourLookup(new SourceBlock(22, 0, 1), new SourceBlock(2, 63, 0)),
            new SingleColourLookup(new SourceBlock(22, 0, 2), new SourceBlock(3, 62, 0)),
            new SingleColourLookup(new SourceBlock(23, 0, 1), new SourceBlock(11, 47, 0)),
            new SingleColourLookup(new SourceBlock(23, 0, 0), new SourceBlock(3, 63, 0)),
            new SingleColourLookup(new SourceBlock(23, 0, 1), new SourceBlock(4, 62, 0)),
            new SingleColourLookup(new SourceBlock(23, 0, 2), new SourceBlock(4, 63, 0)),
            new SingleColourLookup(new SourceBlock(24, 0, 1), new SourceBlock(13, 46, 0)),
            new SingleColourLookup(new SourceBlock(24, 0, 0), new SourceBlock(5, 62, 0)),
            new SingleColourLookup(new SourceBlock(24, 0, 1), new SourceBlock(5, 63, 0)),
            new SingleColourLookup(new SourceBlock(24, 0, 2), new SourceBlock(6, 62, 0)),
            new SingleColourLookup(new SourceBlock(25, 0, 1), new SourceBlock(14, 47, 0)),
            new SingleColourLookup(new SourceBlock(25, 0, 0), new SourceBlock(6, 63, 0)),
            new SingleColourLookup(new SourceBlock(25, 0, 1), new SourceBlock(7, 62, 0)),
            new SingleColourLookup(new SourceBlock(25, 0, 2), new SourceBlock(7, 63, 0)),
            new SingleColourLookup(new SourceBlock(26, 0, 1), new SourceBlock(16, 45, 0)),
            new SingleColourLookup(new SourceBlock(26, 0, 0), new SourceBlock(8, 62, 0)),
            new SingleColourLookup(new SourceBlock(26, 0, 1), new SourceBlock(8, 63, 0)),
            new SingleColourLookup(new SourceBlock(26, 0, 2), new SourceBlock(9, 62, 0)),
            new SingleColourLookup(new SourceBlock(27, 0, 1), new SourceBlock(16, 48, 0)),
            new SingleColourLookup(new SourceBlock(27, 0, 0), new SourceBlock(9, 63, 0)),
            new SingleColourLookup(new SourceBlock(27, 0, 1), new SourceBlock(10, 62, 0)),
            new SingleColourLookup(new SourceBlock(27, 0, 2), new SourceBlock(10, 63, 0)),
            new SingleColourLookup(new SourceBlock(28, 0, 1), new SourceBlock(16, 51, 0)),
            new SingleColourLookup(new SourceBlock(28, 0, 0), new SourceBlock(11, 62, 0)),
            new SingleColourLookup(new SourceBlock(28, 0, 1), new SourceBlock(11, 63, 0)),
            new SingleColourLookup(new SourceBlock(28, 0, 2), new SourceBlock(12, 62, 0)),
            new SingleColourLookup(new SourceBlock(29, 0, 1), new SourceBlock(16, 54, 0)),
            new SingleColourLookup(new SourceBlock(29, 0, 0), new SourceBlock(12, 63, 0)),
            new SingleColourLookup(new SourceBlock(29, 0, 1), new SourceBlock(13, 62, 0)),
            new SingleColourLookup(new SourceBlock(29, 0, 2), new SourceBlock(13, 63, 0)),
            new SingleColourLookup(new SourceBlock(30, 0, 1), new SourceBlock(16, 57, 0)),
            new SingleColourLookup(new SourceBlock(30, 0, 0), new SourceBlock(14, 62, 0)),
            new SingleColourLookup(new SourceBlock(30, 0, 1), new SourceBlock(14, 63, 0)),
            new SingleColourLookup(new SourceBlock(30, 0, 2), new SourceBlock(15, 62, 0)),
            new SingleColourLookup(new SourceBlock(31, 0, 1), new SourceBlock(16, 60, 0)),
            new SingleColourLookup(new SourceBlock(31, 0, 0), new SourceBlock(15, 63, 0)),
            new SingleColourLookup(new SourceBlock(31, 0, 1), new SourceBlock(24, 46, 0)),
            new SingleColourLookup(new SourceBlock(31, 0, 2), new SourceBlock(16, 62, 0)),
            new SingleColourLookup(new SourceBlock(32, 0, 2), new SourceBlock(16, 63, 0)),
            new SingleColourLookup(new SourceBlock(32, 0, 1), new SourceBlock(17, 62, 0)),
            new SingleColourLookup(new SourceBlock(32, 0, 0), new SourceBlock(25, 47, 0)),
            new SingleColourLookup(new SourceBlock(32, 0, 1), new SourceBlock(17, 63, 0)),
            new SingleColourLookup(new SourceBlock(32, 0, 2), new SourceBlock(18, 62, 0)),
            new SingleColourLookup(new SourceBlock(33, 0, 1), new SourceBlock(18, 63, 0)),
            new SingleColourLookup(new SourceBlock(33, 0, 0), new SourceBlock(27, 46, 0)),
            new SingleColourLookup(new SourceBlock(33, 0, 1), new SourceBlock(19, 62, 0)),
            new SingleColourLookup(new SourceBlock(33, 0, 2), new SourceBlock(19, 63, 0)),
            new SingleColourLookup(new SourceBlock(34, 0, 1), new SourceBlock(20, 62, 0)),
            new SingleColourLookup(new SourceBlock(34, 0, 0), new SourceBlock(28, 47, 0)),
            new SingleColourLookup(new SourceBlock(34, 0, 1), new SourceBlock(20, 63, 0)),
            new SingleColourLookup(new SourceBlock(34, 0, 2), new SourceBlock(21, 62, 0)),
            new SingleColourLookup(new SourceBlock(35, 0, 1), new SourceBlock(21, 63, 0)),
            new SingleColourLookup(new SourceBlock(35, 0, 0), new SourceBlock(30, 46, 0)),
            new SingleColourLookup(new SourceBlock(35, 0, 1), new SourceBlock(22, 62, 0)),
            new SingleColourLookup(new SourceBlock(35, 0, 2), new SourceBlock(22, 63, 0)),
            new SingleColourLookup(new SourceBlock(36, 0, 1), new SourceBlock(23, 62, 0)),
            new SingleColourLookup(new SourceBlock(36, 0, 0), new SourceBlock(31, 47, 0)),
            new SingleColourLookup(new SourceBlock(36, 0, 1), new SourceBlock(23, 63, 0)),
            new SingleColourLookup(new SourceBlock(36, 0, 2), new SourceBlock(24, 62, 0)),
            new SingleColourLookup(new SourceBlock(37, 0, 1), new SourceBlock(24, 63, 0)),
            new SingleColourLookup(new SourceBlock(37, 0, 0), new SourceBlock(32, 47, 0)),
            new SingleColourLookup(new SourceBlock(37, 0, 1), new SourceBlock(25, 62, 0)),
            new SingleColourLookup(new SourceBlock(37, 0, 2), new SourceBlock(25, 63, 0)),
            new SingleColourLookup(new SourceBlock(38, 0, 1), new SourceBlock(26, 62, 0)),
            new SingleColourLookup(new SourceBlock(38, 0, 0), new SourceBlock(32, 50, 0)),
            new SingleColourLookup(new SourceBlock(38, 0, 1), new SourceBlock(26, 63, 0)),
            new SingleColourLookup(new SourceBlock(38, 0, 2), new SourceBlock(27, 62, 0)),
            new SingleColourLookup(new SourceBlock(39, 0, 1), new SourceBlock(27, 63, 0)),
            new SingleColourLookup(new SourceBlock(39, 0, 0), new SourceBlock(32, 53, 0)),
            new SingleColourLookup(new SourceBlock(39, 0, 1), new SourceBlock(28, 62, 0)),
            new SingleColourLookup(new SourceBlock(39, 0, 2), new SourceBlock(28, 63, 0)),
            new SingleColourLookup(new SourceBlock(40, 0, 1), new SourceBlock(29, 62, 0)),
            new SingleColourLookup(new SourceBlock(40, 0, 0), new SourceBlock(32, 56, 0)),
            new SingleColourLookup(new SourceBlock(40, 0, 1), new SourceBlock(29, 63, 0)),
            new SingleColourLookup(new SourceBlock(40, 0, 2), new SourceBlock(30, 62, 0)),
            new SingleColourLookup(new SourceBlock(41, 0, 1), new SourceBlock(30, 63, 0)),
            new SingleColourLookup(new SourceBlock(41, 0, 0), new SourceBlock(32, 59, 0)),
            new SingleColourLookup(new SourceBlock(41, 0, 1), new SourceBlock(31, 62, 0)),
            new SingleColourLookup(new SourceBlock(41, 0, 2), new SourceBlock(31, 63, 0)),
            new SingleColourLookup(new SourceBlock(42, 0, 1), new SourceBlock(32, 61, 0)),
            new SingleColourLookup(new SourceBlock(42, 0, 0), new SourceBlock(32, 62, 0)),
            new SingleColourLookup(new SourceBlock(42, 0, 1), new SourceBlock(32, 63, 0)),
            new SingleColourLookup(new SourceBlock(42, 0, 2), new SourceBlock(41, 46, 0)),
            new SingleColourLookup(new SourceBlock(43, 0, 1), new SourceBlock(33, 62, 0)),
            new SingleColourLookup(new SourceBlock(43, 0, 0), new SourceBlock(33, 63, 0)),
            new SingleColourLookup(new SourceBlock(43, 0, 1), new SourceBlock(34, 62, 0)),
            new SingleColourLookup(new SourceBlock(43, 0, 2), new SourceBlock(42, 47, 0)),
            new SingleColourLookup(new SourceBlock(44, 0, 1), new SourceBlock(34, 63, 0)),
            new SingleColourLookup(new SourceBlock(44, 0, 0), new SourceBlock(35, 62, 0)),
            new SingleColourLookup(new SourceBlock(44, 0, 1), new SourceBlock(35, 63, 0)),
            new SingleColourLookup(new SourceBlock(44, 0, 2), new SourceBlock(44, 46, 0)),
            new SingleColourLookup(new SourceBlock(45, 0, 1), new SourceBlock(36, 62, 0)),
            new SingleColourLookup(new SourceBlock(45, 0, 0), new SourceBlock(36, 63, 0)),
            new SingleColourLookup(new SourceBlock(45, 0, 1), new SourceBlock(37, 62, 0)),
            new SingleColourLookup(new SourceBlock(45, 0, 2), new SourceBlock(45, 47, 0)),
            new SingleColourLookup(new SourceBlock(46, 0, 1), new SourceBlock(37, 63, 0)),
            new SingleColourLookup(new SourceBlock(46, 0, 0), new SourceBlock(38, 62, 0)),
            new SingleColourLookup(new SourceBlock(46, 0, 1), new SourceBlock(38, 63, 0)),
            new SingleColourLookup(new SourceBlock(46, 0, 2), new SourceBlock(47, 46, 0)),
            new SingleColourLookup(new SourceBlock(47, 0, 1), new SourceBlock(39, 62, 0)),
            new SingleColourLookup(new SourceBlock(47, 0, 0), new SourceBlock(39, 63, 0)),
            new SingleColourLookup(new SourceBlock(47, 0, 1), new SourceBlock(40, 62, 0)),
            new SingleColourLookup(new SourceBlock(47, 0, 2), new SourceBlock(48, 46, 0)),
            new SingleColourLookup(new SourceBlock(48, 0, 2), new SourceBlock(40, 63, 0)),
            new SingleColourLookup(new SourceBlock(48, 0, 1), new SourceBlock(41, 62, 0)),
            new SingleColourLookup(new SourceBlock(48, 0, 0), new SourceBlock(41, 63, 0)),
            new SingleColourLookup(new SourceBlock(48, 0, 1), new SourceBlock(48, 49, 0)),
            new SingleColourLookup(new SourceBlock(48, 0, 2), new SourceBlock(42, 62, 0)),
            new SingleColourLookup(new SourceBlock(49, 0, 1), new SourceBlock(42, 63, 0)),
            new SingleColourLookup(new SourceBlock(49, 0, 0), new SourceBlock(43, 62, 0)),
            new SingleColourLookup(new SourceBlock(49, 0, 1), new SourceBlock(48, 52, 0)),
            new SingleColourLookup(new SourceBlock(49, 0, 2), new SourceBlock(43, 63, 0)),
            new SingleColourLookup(new SourceBlock(50, 0, 1), new SourceBlock(44, 62, 0)),
            new SingleColourLookup(new SourceBlock(50, 0, 0), new SourceBlock(44, 63, 0)),
            new SingleColourLookup(new SourceBlock(50, 0, 1), new SourceBlock(48, 55, 0)),
            new SingleColourLookup(new SourceBlock(50, 0, 2), new SourceBlock(45, 62, 0)),
            new SingleColourLookup(new SourceBlock(51, 0, 1), new SourceBlock(45, 63, 0)),
            new SingleColourLookup(new SourceBlock(51, 0, 0), new SourceBlock(46, 62, 0)),
            new SingleColourLookup(new SourceBlock(51, 0, 1), new SourceBlock(48, 58, 0)),
            new SingleColourLookup(new SourceBlock(51, 0, 2), new SourceBlock(46, 63, 0)),
            new SingleColourLookup(new SourceBlock(52, 0, 1), new SourceBlock(47, 62, 0)),
            new SingleColourLookup(new SourceBlock(52, 0, 0), new SourceBlock(47, 63, 0)),
            new SingleColourLookup(new SourceBlock(52, 0, 1), new SourceBlock(48, 61, 0)),
            new SingleColourLookup(new SourceBlock(52, 0, 2), new SourceBlock(48, 62, 0)),
            new SingleColourLookup(new SourceBlock(53, 0, 1), new SourceBlock(56, 47, 0)),
            new SingleColourLookup(new SourceBlock(53, 0, 0), new SourceBlock(48, 63, 0)),
            new SingleColourLookup(new SourceBlock(53, 0, 1), new SourceBlock(49, 62, 0)),
            new SingleColourLookup(new SourceBlock(53, 0, 2), new SourceBlock(49, 63, 0)),
            new SingleColourLookup(new SourceBlock(54, 0, 1), new SourceBlock(58, 46, 0)),
            new SingleColourLookup(new SourceBlock(54, 0, 0), new SourceBlock(50, 62, 0)),
            new SingleColourLookup(new SourceBlock(54, 0, 1), new SourceBlock(50, 63, 0)),
            new SingleColourLookup(new SourceBlock(54, 0, 2), new SourceBlock(51, 62, 0)),
            new SingleColourLookup(new SourceBlock(55, 0, 1), new SourceBlock(59, 47, 0)),
            new SingleColourLookup(new SourceBlock(55, 0, 0), new SourceBlock(51, 63, 0)),
            new SingleColourLookup(new SourceBlock(55, 0, 1), new SourceBlock(52, 62, 0)),
            new SingleColourLookup(new SourceBlock(55, 0, 2), new SourceBlock(52, 63, 0)),
            new SingleColourLookup(new SourceBlock(56, 0, 1), new SourceBlock(61, 46, 0)),
            new SingleColourLookup(new SourceBlock(56, 0, 0), new SourceBlock(53, 62, 0)),
            new SingleColourLookup(new SourceBlock(56, 0, 1), new SourceBlock(53, 63, 0)),
            new SingleColourLookup(new SourceBlock(56, 0, 2), new SourceBlock(54, 62, 0)),
            new SingleColourLookup(new SourceBlock(57, 0, 1), new SourceBlock(62, 47, 0)),
            new SingleColourLookup(new SourceBlock(57, 0, 0), new SourceBlock(54, 63, 0)),
            new SingleColourLookup(new SourceBlock(57, 0, 1), new SourceBlock(55, 62, 0)),
            new SingleColourLookup(new SourceBlock(57, 0, 2), new SourceBlock(55, 63, 0)),
            new SingleColourLookup(new SourceBlock(58, 0, 1), new SourceBlock(56, 62, 1)),
            new SingleColourLookup(new SourceBlock(58, 0, 0), new SourceBlock(56, 62, 0)),
            new SingleColourLookup(new SourceBlock(58, 0, 1), new SourceBlock(56, 63, 0)),
            new SingleColourLookup(new SourceBlock(58, 0, 2), new SourceBlock(57, 62, 0)),
            new SingleColourLookup(new SourceBlock(59, 0, 1), new SourceBlock(57, 63, 1)),
            new SingleColourLookup(new SourceBlock(59, 0, 0), new SourceBlock(57, 63, 0)),
            new SingleColourLookup(new SourceBlock(59, 0, 1), new SourceBlock(58, 62, 0)),
            new SingleColourLookup(new SourceBlock(59, 0, 2), new SourceBlock(58, 63, 0)),
            new SingleColourLookup(new SourceBlock(60, 0, 1), new SourceBlock(59, 62, 1)),
            new SingleColourLookup(new SourceBlock(60, 0, 0), new SourceBlock(59, 62, 0)),
            new SingleColourLookup(new SourceBlock(60, 0, 1), new SourceBlock(59, 63, 0)),
            new SingleColourLookup(new SourceBlock(60, 0, 2), new SourceBlock(60, 62, 0)),
            new SingleColourLookup(new SourceBlock(61, 0, 1), new SourceBlock(60, 63, 1)),
            new SingleColourLookup(new SourceBlock(61, 0, 0), new SourceBlock(60, 63, 0)),
            new SingleColourLookup(new SourceBlock(61, 0, 1), new SourceBlock(61, 62, 0)),
            new SingleColourLookup(new SourceBlock(61, 0, 2), new SourceBlock(61, 63, 0)),
            new SingleColourLookup(new SourceBlock(62, 0, 1), new SourceBlock(62, 62, 1)),
            new SingleColourLookup(new SourceBlock(62, 0, 0), new SourceBlock(62, 62, 0)),
            new SingleColourLookup(new SourceBlock(62, 0, 1), new SourceBlock(62, 63, 0)),
            new SingleColourLookup(new SourceBlock(62, 0, 2), new SourceBlock(63, 62, 0)),
            new SingleColourLookup(new SourceBlock(63, 0, 1), new SourceBlock(63, 63, 1)),
            new SingleColourLookup(new SourceBlock(63, 0, 0), new SourceBlock(63, 63, 0))
        };
    }
    public struct SourceBlock
    {
        public byte start;
        public byte end;
        public byte error;

        public SourceBlock(byte s, byte e, byte err)
        {
            start = s;
            end = e;
            error = err;
        }
    };
    public class Sym3x3
    {
        private float[] X { get; set; } = new float[6];

        public float this[int i]
        {
            get => X[i];
            set => X[i] = value;
        }

        public Sym3x3(float s)
        {
            for (int i = 0; i < 6; i++) { X[i] = s; }
        }

        public static Sym3x3 ComputeWeightedCovariance(int n, Vector3[] points, float[] weights)
        {
            // compute the centroid
            float total = 0.0f;
            Vector3 centroid = new Vector3(0.0f, 0.0f, 0.0f);

            for (int i = 0; i < n; ++i)
            {
                total += weights[i];
                centroid += weights[i] * points[i];
            }

            if (total > float.Epsilon) { centroid /= total; }

            // accumulate the covariance matrix
            Sym3x3 covariance = new Sym3x3(0.0f);

            for (int i = 0; i < n; ++i)
            {
                Vector3 a = points[i] - centroid;
                Vector3 b = weights[i] * a;

                covariance[0] += a.x * b.x;
                covariance[1] += a.x * b.y;
                covariance[2] += a.x * b.z;
                covariance[3] += a.y * b.y;
                covariance[4] += a.y * b.z;
                covariance[5] += a.z * b.z;
            }

            // return it
            return covariance;
        }

        public static Vector3 ComputePrincipleComponent(Sym3x3 matrix)
        {
            Vector4 row0 = new Vector4(matrix[0], matrix[1], matrix[2], 0.0f);
            Vector4 row1 = new Vector4(matrix[1], matrix[3], matrix[4], 0.0f);
            Vector4 row2 = new Vector4(matrix[2], matrix[4], matrix[5], 0.0f);
            Vector4 v = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

            for (int i = 0; i < 8; ++i)
            {
                // matrix multiply
                Vector4 w = Vector4.Scale(row0 , v.SplatX());
                w = Helpers.MultiplyAdd(row1, v.SplatY(), w);
                w = Helpers.MultiplyAdd(row2, v.SplatZ(), w);

                // get max component from xyz in all channels
                Vector4 a = Vector4.Max(w.SplatX(), Vector4.Max(w.SplatY(), w.SplatZ()));

                // divide through and advance
                v = Vector4.Scale(w , Helpers.Reciprocal(a));
            }

            return v.ToVector3();
        }
    }


    

    public static class Helpers
    {
        public static Vector4 MultiplyAdd(Vector4 a, Vector4 b, Vector4 c)
        {
            return Vector4.Scale(a , b) + c;
        }

        public static Vector4 NegativeMultiplySubtract(Vector4 a, Vector4 b, Vector4 c)
        {
            return c - Vector4.Scale(a , b);
        }

        public static Vector4 Reciprocal(Vector4 v)
        {
            return new Vector4(
                    1.0f / v.x,
                    1.0f / v.y,
                    1.0f / v.z,
                    1.0f / v.w
            );
        }

        public static Vector4 Truncate(Vector4 v)
        {
            return new Vector4(
                (float)(v.x > 0.0f ? Math.Floor(v.x) : Math.Ceiling(v.x)),
                (float)(v.y > 0.0f ? Math.Floor(v.y) : Math.Ceiling(v.y)),
                (float)(v.z > 0.0f ? Math.Floor(v.z) : Math.Ceiling(v.z)),
                (float)(v.w > 0.0f ? Math.Floor(v.w) : Math.Ceiling(v.w))
            );
        }

        public static Vector3 Truncate(Vector3 v)
        {
            return new Vector3(
                (float)(v.x > 0.0f ? Math.Floor(v.x) : Math.Ceiling(v.x)),
                (float)(v.y > 0.0f ? Math.Floor(v.y) : Math.Ceiling(v.y)),
                (float)(v.z > 0.0f ? Math.Floor(v.z) : Math.Ceiling(v.z))
            );
        }

        public static bool CompareAnyLessThan(Vector4 left, Vector4 right)
        {
            return left.x < right.x ||
                    left.y < right.y ||
                    left.z < right.z ||
                    left.w < right.w;
        }

        public static Vector4 SplatX(this Vector4 v) { return new Vector4(v.x, v.x, v.x, v.x); }

        public static Vector4 SplatY(this Vector4 v) { return new Vector4(v.y, v.y, v.y, v.y); }

        public static Vector4 SplatZ(this Vector4 v) { return new Vector4(v.z, v.z, v.z, v.z); }

        public static Vector4 SplatW(this Vector4 v) { return new Vector4(v.w, v.w, v.w, v.w); }


        public static Vector3 ToVector3(this Vector4 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }
    }
}
