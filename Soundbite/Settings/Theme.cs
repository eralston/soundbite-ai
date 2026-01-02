using Masticore;

namespace Soundbite.Settings
{
    /// <summary>
    /// Based on the <see href="https://getbootstrap.com/docs/4.0/utilities/colors/">semantic colors in Bootstrap</see>
    /// </summary>
    [CodeGenModel]
    public class ThemeBootstrap
    {
        public string Default { get; set; }
        [CodeGenField(IsNullable = true)]
        public string DefaultHigh { get; set; }
        [CodeGenField(IsNullable = true)]
        public string DefaultLow { get; set; }

        public string Primary { get; set; }
        [CodeGenField(IsNullable = true)]
        public string PrimaryHigh { get; set; }
        [CodeGenField(IsNullable = true)]
        public string PrimaryLow { get; set; }

        public string Secondary { get; set; }
        [CodeGenField(IsNullable = true)]
        public string SecondaryHigh { get; set; }
        [CodeGenField(IsNullable = true)]
        public string SecondaryLow { get; set; }

        public string Info { get; set; }
        [CodeGenField(IsNullable = true)]
        public string InfoHigh { get; set; }
        [CodeGenField(IsNullable = true)]
        public string InfoLow { get; set; }

        public string Success { get; set; }
        [CodeGenField(IsNullable = true)]
        public string SuccessHigh { get; set; }
        [CodeGenField(IsNullable = true)]
        public string SuccessLow { get; set; }

        public string Danger { get; set; }
        [CodeGenField(IsNullable = true)]
        public string DangerHigh { get; set; }
        [CodeGenField(IsNullable = true)]
        public string DangerLow { get; set; }

        public string Warning { get; set; }
        [CodeGenField(IsNullable = true)]
        public string WarningHigh { get; set; }
        [CodeGenField(IsNullable = true)]
        public string WarningLow { get; set; }
    };

    /// <summary>
    /// The set of colors required to the convey the unique branding of a company
    /// </summary>
    [CodeGenModel]
    public class ThemeBrand
    {
        public string First { get; set; }
        public string Second { get; set; }
    }

    /// <summary>
    /// The light and dark shades that provide visual structure for the theme
    /// </summary>
    [CodeGenModel]
    public class ThemeNeutrals
    {
        /// <summary> 
        /// Color for the background of the app, EG backgroundColor on body
        /// </summary>
        public string Background { get; set; }

        /// <summary> 
        /// Color for elements on top of the background, but still acting as a holder, EG, the main nav
        /// </summary>
        public string Midground { get; set; }

        /// <summary> 
        /// Color for elements on the top-most non-information layer EG, popup menus or active inputs
        /// </summary>
        public string Foreground { get; set; }

        /// <summary> 
        /// A stand-in for the expected "white" in the color system; though in a mostly dark theme, this would be closest to black
        /// </summary>
        public string Min { get; set; } // Normally white

        /// <summary>
        /// Slight difference from Min, used against opposing backgrounds
        /// </summary>
        public string N100 { get; set; }

        /// <summary> 
        /// Bootstrap "light" color, like in text-light
        /// </summary>
        public string N200 { get; set; }

        /// <summary> 
        /// Mostly border colors where one wants AA contrast from min
        /// </summary>
        public string N300 { get; set; }

        /// <summary> 
        /// 
        /// </summary>
        public string N400 { get; set; }

        /// <summary>
        /// Beginning
        /// </summary>
        public string N500 { get; set; }

        /// <summary> 
        /// Bootstrap "dark" color, such as in text-dark
        /// </summary>
        public string N600 { get; set; }

        /// <summary> 
        /// Gray; body color
        /// </summary>
        public string N700 { get; set; }

        /// <summary> 
        /// Gray-dark, header colors
        /// </summary>
        public string N800 { get; set; }

        /// <summary> 
        /// Darkest; usually a typographic color
        /// </summary>
        public string N900 { get; set; }

        /// <summary> 
        /// Stand in for the darkest neutral; In a light palette, this would be black-ish
        /// </summary>
        public string Max { get; set; }
    }

    /// <summary>
    /// The pure color hues in the application; distinct from Bootstrap colors, but si
    /// </summary>
    [CodeGenModel]
    public class ThemeHues
    {
        public string Red { get; set; }
        public string Orange { get; set; }
        public string Yellow { get; set; }
        public string Green { get; set; }
        public string Blue { get; set; }
        public string Indigo { get; set; }
        public string Violet { get; set; }
        public string Pink { get; set; }
        public string Teal { get; set; }
        public string Cyan { get; set; }
    }

    /// <summary>
    ///Declares the set of colors required to describe an interface
    /// </summary>
    [CodeGenModel]
    public class ThemeColors
    {
        /// <summary> Spectrum of tones for creating depth - black to white with some subtle coloring/// </summary>
        public ThemeNeutrals Neutrals { get; set; }
        public ThemeBrand Brand { get; set; }
        public ThemeBootstrap Bootstrap { get; set; }
        public ThemeHues Hues { get; set; }

        /// <summary> 
        /// Color used on transparenty elements - usually just "transparent"
        /// </summary>
        public string Transparent { get; set; }

    };

    /// <summary>
    /// The set of logo images for the local client
    /// </summary>
    [CodeGenModel]
    public class ThemeImages
    {
        /// <summary>
        /// Full size logo for the main views
        /// </summary>
        public string Logo { get; set; }

        /// <summary> 
        /// Full size logo with alternate colors for opposing shade backgrounds
        /// </summary>
        public string LogoAlt { get; set; }

        /// <summary>
        /// A small version of the logo, usually just a symbol, for narrow views
        /// </summary>
        public string Mark { get; set; }
    }

    /// <summary>
    ///Declaration for the font system
    /// </summary>
    [CodeGenModel]
    public class ThemeFonts
    {
        public string FontFamily { get; set; }
        public string FontFamilyMono { get; set; }
    }

    /// <summary>
    ///Description for a complete theme
    /// </summary>
    [CodeGenModel]
    public class Theme
    {
        public string Name { get; set; }

        [CodeGenField(IsNullable = true)]
        public string PublishedUtc { get; set; }

        public ThemeImages Images { get; set; }

        public ThemeColors Colors { get; set; }

        public ThemeFonts Fonts { get; set; }
    }
}
