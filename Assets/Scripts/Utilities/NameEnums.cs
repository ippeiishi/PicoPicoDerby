using System;

namespace uPalette.Generated
{
    public enum ColorTheme
    {
        Default,
        Test,
    }

    public static class ColorThemeExtensions
    {
        public static string ToThemeId(this ColorTheme theme)
        {
            switch (theme)
            {
                case ColorTheme.Default:
                    return "192c4843-0369-43dd-8397-ada1627023dc";
                case ColorTheme.Test:
                    return "b2f10199-4ad9-4d26-9517-a311bf37a84c";
                default:
                    throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
            }
        }
    }

    public enum ColorEntry
    {
        UIPanelBase,
        UIPanelValueBG,
        TextMain,
        UIBGAlpha,
        UIMainColor,
        UIOff,
        BtnOK,
        UIOffBG,
        UIPureWhite,
        BtnCancel,
        BtnBack,
        NewColor,
    }

    public static class ColorEntryExtensions
    {
        public static string ToEntryId(this ColorEntry entry)
        {
            switch (entry)
            {
                case ColorEntry.UIPanelBase:
                    return "086b101f-79fd-40ef-b5a3-051811dc97f4";
                case ColorEntry.UIPanelValueBG:
                    return "0e59a5b3-a459-47ae-9b00-c467b6747976";
                case ColorEntry.TextMain:
                    return "302b9574-3326-4487-976f-ca16071cb03c";
                case ColorEntry.UIBGAlpha:
                    return "ce346692-81d7-4368-9697-23c8fc975617";
                case ColorEntry.UIMainColor:
                    return "5e455ffa-f48e-449b-9085-91cc5dd0fdae";
                case ColorEntry.UIOff:
                    return "f694beb3-ffdb-4ce5-8900-ca0ee7fe8435";
                case ColorEntry.BtnOK:
                    return "6fa0272c-e1c0-4655-ac34-9cb163bbdaf7";
                case ColorEntry.UIOffBG:
                    return "a8d10193-d475-4461-918f-7fa9a29f6aea";
                case ColorEntry.UIPureWhite:
                    return "e3d259ec-e394-4797-9849-984ee0f304fe";
                case ColorEntry.BtnCancel:
                    return "a0e5935e-fbb8-4ee7-8a91-f19d8a9d7709";
                case ColorEntry.BtnBack:
                    return "98f1f6ee-83c5-4863-ad7b-cdae0c94c88c";
                case ColorEntry.NewColor:
                    return "41f860d4-18e2-4992-9d6c-c4ade39d5bde";
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), entry, null);
            }
        }
    }

    public enum GradientTheme
    {
        Default,
    }

    public static class GradientThemeExtensions
    {
        public static string ToThemeId(this GradientTheme theme)
        {
            switch (theme)
            {
                case GradientTheme.Default:
                    return "7a3b62ae-05d8-46f5-b12d-f8dae79006c8";
                default:
                    throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
            }
        }
    }

    public enum GradientEntry
    {
    }

    public static class GradientEntryExtensions
    {
        public static string ToEntryId(this GradientEntry entry)
        {
            switch (entry)
            {
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), entry, null);
            }
        }
    }

    public enum CharacterStyleTheme
    {
        Default,
    }

    public static class CharacterStyleThemeExtensions
    {
        public static string ToThemeId(this CharacterStyleTheme theme)
        {
            switch (theme)
            {
                case CharacterStyleTheme.Default:
                    return "4f4f96db-8413-4791-a037-fd6ed5e4bf6b";
                default:
                    throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
            }
        }
    }

    public enum CharacterStyleEntry
    {
    }

    public static class CharacterStyleEntryExtensions
    {
        public static string ToEntryId(this CharacterStyleEntry entry)
        {
            switch (entry)
            {
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), entry, null);
            }
        }
    }

    public enum CharacterStyleTMPTheme
    {
        Default,
    }

    public static class CharacterStyleTMPThemeExtensions
    {
        public static string ToThemeId(this CharacterStyleTMPTheme theme)
        {
            switch (theme)
            {
                case CharacterStyleTMPTheme.Default:
                    return "27f1713b-0dc1-4279-b5d7-24d880d2c1dd";
                default:
                    throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
            }
        }
    }

    public enum CharacterStyleTMPEntry
    {
    }

    public static class CharacterStyleTMPEntryExtensions
    {
        public static string ToEntryId(this CharacterStyleTMPEntry entry)
        {
            switch (entry)
            {
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), entry, null);
            }
        }
    }
}
