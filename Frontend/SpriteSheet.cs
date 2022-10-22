using Raylib_CsLo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CitySim.Frontend
{
    [XmlRoot("TextureAtlas")]
    public class SpriteSheetXML
    {
        [XmlElement("SubTexture")]
        public List<SpriteSheetXmlEntry>? Entries { get; set; }
    }

    public class SpriteSheetXmlEntry
    {
        [XmlAttribute("name")]
        public string? Name { get; set; }
        [XmlAttribute("x")]
        public int X { get; set; }
        [XmlAttribute("y")]
        public int Y { get; set; }
        [XmlAttribute("width")]
        public int Width { get; set; }
        [XmlAttribute("height")]
        public int Height { get; set; }
    }

    internal class SpriteSheet
    {
        public IReadOnlyDictionary<string, Rectangle> Rects => _rects;

        private Dictionary<string, Rectangle> _rects = new Dictionary<string, Rectangle>();
        private readonly Texture _texture;

        public SpriteSheet(Texture texture, params (string name, Rectangle rect)[] rects)
        {
            _texture = texture;

            foreach (var (name, rect) in rects)
            {
                _rects.Add(name, rect);
            }
        }

        public void DrawSprite(string name, Vector2 position)
        {
            Rectangle sourceRect = _rects[name];

            Raylib.DrawTexturePro(_texture, sourceRect,
                new Rectangle(position.X, position.Y, sourceRect.width, sourceRect.height), 
                Vector2.Zero, 0, Raylib.WHITE);
        }

        public static SpriteSheet FromPNG_XML(string pngFileName, string xmlFileName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SpriteSheetXML));

            SpriteSheetXML? result;

            using (FileStream fileStream = new FileStream(xmlFileName, FileMode.Open))
            {
                result = (SpriteSheetXML?)serializer.Deserialize(fileStream);
            }

            var rects = result!.Entries!.Select(entry => (
                entry.Name,
                new Rectangle(entry.X, entry.Y, entry.Width, entry.Height)
            )).ToArray();

            return new SpriteSheet(Raylib.LoadTexture(pngFileName) , rects!);
        }
    }
}
