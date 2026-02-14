using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

public class NCGRViewer : Form
{
    private PictureBox pictureBox;
    private Bitmap bitmap;
    private const int TILE_SIZE = 8;
    private const int TILES_PER_ROW = 5;
    private const int IMAGE_SIZE = TILE_SIZE * TILES_PER_ROW;
    private Color[] palette = new Color[16];

    public NCGRViewer()
    {
        this.Text = "NCGR Viewer";
        this.ClientSize = new Size(IMAGE_SIZE, IMAGE_SIZE);

        pictureBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.StretchImage
        };

        this.Controls.Add(pictureBox);
        LoadNCGR("d2_ObjKeepData.bin");
    }

    private void LoadNCGR(string filePath)
    {
        byte[] data = File.ReadAllBytes(filePath);

        // パレットデコード
        for (int i = 0; i < 16; i++)
        {
            int offset = 0x5E50 + (i * 2); // カラーパレットのオフセット
            ushort colorData = BitConverter.ToUInt16(data, offset);
            int r = (colorData & 0x1F) * 8;
            int g = ((colorData >> 5) & 0x1F) * 8;
            int b = ((colorData >> 10) & 0x1F) * 8;
            palette[i] = Color.FromArgb(r, g, b);
        }

        // 画像サイズを決定
        int TILE_SIZE = 8;
        int XTILES_PER_ROW = 5;

        int numBlocks = 3; //タイル数

        int YTILES_PER_ROW = numBlocks * 5;
        int TILE_COUNT = XTILES_PER_ROW * YTILES_PER_ROW; // タイル総数
        int IMAGE_XSIZE = TILE_SIZE * XTILES_PER_ROW;
        int IMAGE_YSIZE = TILE_SIZE * YTILES_PER_ROW;

        Bitmap bitmap = new Bitmap(IMAGE_XSIZE, IMAGE_YSIZE);

        for (int blockIndex = 0; blockIndex < numBlocks; blockIndex++)
        {
            for (int tile = 0; tile < 25; tile++)
            {
                int newTileIndex = tile;

                // タイルの並び替え
                if (tile < 20)
                {
                    newTileIndex = ((tile % 4) * 5) + (tile / 4);
                    if (tile < 16)
                    {
                        newTileIndex = ((tile / 4) * 5) + (tile % 4);
                    }
                }

                int tileX = (newTileIndex % XTILES_PER_ROW) * TILE_SIZE;
                int tileY = ((newTileIndex / XTILES_PER_ROW) * TILE_SIZE) + (blockIndex * 40);

                int Boffset = 0x20C8+ (0x960 * 4 ) + ((blockIndex * 25 + tile) * 32);

                for (int y = 0; y < TILE_SIZE; y++)
                {
                    for (int x = 0; x < TILE_SIZE; x += 2)
                    {
                        if (Boffset + (y * 4) + (x / 2) >= data.Length)
                            continue; // 範囲外ならスキップ

                        byte pixelData = data[Boffset + (y * 4) + (x / 2)];

                        byte index1 = (byte)(pixelData & 0x0F);
                        byte index2 = (byte)((pixelData >> 4) & 0x0F);

                        bitmap.SetPixel(tileX + x, tileY + y, palette[index1]);
                        bitmap.SetPixel(tileX + x + 1, tileY + y, palette[index2]);
                    }
                }
            }
        }
        pictureBox.Image = bitmap;
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new NCGRViewer());
    }
}

