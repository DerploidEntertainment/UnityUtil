﻿using NUnit.Framework;

namespace UnityUtil.Editor.Tests.Editor;

public class AsciiSpritesDrawerTest : BaseEditModeTestFixture
{
    [Test]
    public void ExpandsDecimalTemplateString()
    {
        string assetName;
        string templateFileName;

        // Using 'dec'
        templateFileName = "char-{dec}.png";
        assetName = AsciiSpritesEditor.GetAssetName(' ', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-32.png"));
        assetName = AsciiSpritesEditor.GetAssetName('3', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-51.png"));
        assetName = AsciiSpritesEditor.GetAssetName('A', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-65.png"));
        assetName = AsciiSpritesEditor.GetAssetName('a', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-97.png"));
        assetName = AsciiSpritesEditor.GetAssetName('~', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-126.png"));

        // Using 'decimal'
        templateFileName = "char-{decimal}.png";
        assetName = AsciiSpritesEditor.GetAssetName(' ', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-32.png"));
        assetName = AsciiSpritesEditor.GetAssetName('3', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-51.png"));
        assetName = AsciiSpritesEditor.GetAssetName('A', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-65.png"));
        assetName = AsciiSpritesEditor.GetAssetName('a', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-97.png"));
        assetName = AsciiSpritesEditor.GetAssetName('~', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-126.png"));

        // Using '10'
        templateFileName = "char-{10}.png";
        assetName = AsciiSpritesEditor.GetAssetName(' ', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-32.png"));
        assetName = AsciiSpritesEditor.GetAssetName('3', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-51.png"));
        assetName = AsciiSpritesEditor.GetAssetName('A', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-65.png"));
        assetName = AsciiSpritesEditor.GetAssetName('a', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-97.png"));
        assetName = AsciiSpritesEditor.GetAssetName('~', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-126.png"));

        // Using mixed template strings
        templateFileName = "char-{dec}-{decimal}-{10}.png";
        assetName = AsciiSpritesEditor.GetAssetName(' ', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-32-32-32.png"));
        assetName = AsciiSpritesEditor.GetAssetName('3', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-51-51-51.png"));
        assetName = AsciiSpritesEditor.GetAssetName('A', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-65-65-65.png"));
        assetName = AsciiSpritesEditor.GetAssetName('a', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-97-97-97.png"));
        assetName = AsciiSpritesEditor.GetAssetName('~', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-126-126-126.png"));
    }

    [Test]
    public void ExpandsHexadecimalTemplateString()
    {
        string assetName;
        string templateFileName;

        // Using 'dec'
        templateFileName = "char-{hex}.png";
        assetName = AsciiSpritesEditor.GetAssetName(' ', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-20.png"));
        assetName = AsciiSpritesEditor.GetAssetName('3', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-33.png"));
        assetName = AsciiSpritesEditor.GetAssetName('A', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-41.png"));
        assetName = AsciiSpritesEditor.GetAssetName('a', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-61.png"));
        assetName = AsciiSpritesEditor.GetAssetName('~', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-7e.png"));

        // Using 'decimal'
        templateFileName = "char-{hexadecimal}.png";
        assetName = AsciiSpritesEditor.GetAssetName(' ', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-20.png"));
        assetName = AsciiSpritesEditor.GetAssetName('3', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-33.png"));
        assetName = AsciiSpritesEditor.GetAssetName('A', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-41.png"));
        assetName = AsciiSpritesEditor.GetAssetName('a', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-61.png"));
        assetName = AsciiSpritesEditor.GetAssetName('~', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-7e.png"));

        // Using '10'
        templateFileName = "char-{16}.png";
        assetName = AsciiSpritesEditor.GetAssetName(' ', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-20.png"));
        assetName = AsciiSpritesEditor.GetAssetName('3', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-33.png"));
        assetName = AsciiSpritesEditor.GetAssetName('A', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-41.png"));
        assetName = AsciiSpritesEditor.GetAssetName('a', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-61.png"));
        assetName = AsciiSpritesEditor.GetAssetName('~', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-7e.png"));

        // Using mixed template strings
        templateFileName = "char-{hex}-{hexadecimal}-{16}.png";
        assetName = AsciiSpritesEditor.GetAssetName(' ', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-20-20-20.png"));
        assetName = AsciiSpritesEditor.GetAssetName('3', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-33-33-33.png"));
        assetName = AsciiSpritesEditor.GetAssetName('A', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-41-41-41.png"));
        assetName = AsciiSpritesEditor.GetAssetName('a', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-61-61-61.png"));
        assetName = AsciiSpritesEditor.GetAssetName('~', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-7e-7e-7e.png"));
    }

    [Test]
    public void ExpandsOctalTemplateString()
    {
        string assetName;
        string templateFileName;

        // Using 'dec'
        templateFileName = "char-{oct}.png";
        assetName = AsciiSpritesEditor.GetAssetName(' ', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-40.png"));
        assetName = AsciiSpritesEditor.GetAssetName('3', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-63.png"));
        assetName = AsciiSpritesEditor.GetAssetName('A', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-101.png"));
        assetName = AsciiSpritesEditor.GetAssetName('a', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-141.png"));
        assetName = AsciiSpritesEditor.GetAssetName('~', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-176.png"));

        // Using 'decimal'
        templateFileName = "char-{octal}.png";
        assetName = AsciiSpritesEditor.GetAssetName(' ', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-40.png"));
        assetName = AsciiSpritesEditor.GetAssetName('3', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-63.png"));
        assetName = AsciiSpritesEditor.GetAssetName('A', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-101.png"));
        assetName = AsciiSpritesEditor.GetAssetName('a', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-141.png"));
        assetName = AsciiSpritesEditor.GetAssetName('~', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-176.png"));

        // Using '10'
        templateFileName = "char-{8}.png";
        assetName = AsciiSpritesEditor.GetAssetName(' ', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-40.png"));
        assetName = AsciiSpritesEditor.GetAssetName('3', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-63.png"));
        assetName = AsciiSpritesEditor.GetAssetName('A', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-101.png"));
        assetName = AsciiSpritesEditor.GetAssetName('a', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-141.png"));
        assetName = AsciiSpritesEditor.GetAssetName('~', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-176.png"));

        // Using mixed template strings
        templateFileName = "char-{oct}-{octal}-{8}.png";
        assetName = AsciiSpritesEditor.GetAssetName(' ', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-40-40-40.png"));
        assetName = AsciiSpritesEditor.GetAssetName('3', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-63-63-63.png"));
        assetName = AsciiSpritesEditor.GetAssetName('A', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-101-101-101.png"));
        assetName = AsciiSpritesEditor.GetAssetName('a', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-141-141-141.png"));
        assetName = AsciiSpritesEditor.GetAssetName('~', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-176-176-176.png"));
    }

    [Test]
    public void ExpandsBinaryTemplateString()
    {
        string assetName;
        string templateFileName;

        // Using 'dec'
        templateFileName = "char-{bin}.png";
        assetName = AsciiSpritesEditor.GetAssetName(' ', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-100000.png"));
        assetName = AsciiSpritesEditor.GetAssetName('3', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-110011.png"));
        assetName = AsciiSpritesEditor.GetAssetName('A', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-1000001.png"));
        assetName = AsciiSpritesEditor.GetAssetName('a', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-1100001.png"));
        assetName = AsciiSpritesEditor.GetAssetName('~', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-1111110.png"));

        // Using 'decimal'
        templateFileName = "char-{binary}.png";
        assetName = AsciiSpritesEditor.GetAssetName(' ', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-100000.png"));
        assetName = AsciiSpritesEditor.GetAssetName('3', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-110011.png"));
        assetName = AsciiSpritesEditor.GetAssetName('A', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-1000001.png"));
        assetName = AsciiSpritesEditor.GetAssetName('a', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-1100001.png"));
        assetName = AsciiSpritesEditor.GetAssetName('~', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-1111110.png"));

        // Using '10'
        templateFileName = "char-{2}.png";
        assetName = AsciiSpritesEditor.GetAssetName(' ', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-100000.png"));
        assetName = AsciiSpritesEditor.GetAssetName('3', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-110011.png"));
        assetName = AsciiSpritesEditor.GetAssetName('A', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-1000001.png"));
        assetName = AsciiSpritesEditor.GetAssetName('a', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-1100001.png"));
        assetName = AsciiSpritesEditor.GetAssetName('~', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-1111110.png"));

        // Using mixed template strings
        templateFileName = "char-{bin}-{binary}-{2}.png";
        assetName = AsciiSpritesEditor.GetAssetName(' ', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-100000-100000-100000.png"));
        assetName = AsciiSpritesEditor.GetAssetName('3', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-110011-110011-110011.png"));
        assetName = AsciiSpritesEditor.GetAssetName('A', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-1000001-1000001-1000001.png"));
        assetName = AsciiSpritesEditor.GetAssetName('a', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-1100001-1100001-1100001.png"));
        assetName = AsciiSpritesEditor.GetAssetName('~', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-1111110-1111110-1111110.png"));
    }

    [Test]
    public void ExpandsMixedBaseTemplateString()
    {
        string assetName;
        string templateFileName;

        // Short form
        templateFileName = "char-{bin}-{oct}-{dec}-{hex}.png";
        assetName = AsciiSpritesEditor.GetAssetName(' ', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-100000-40-32-20.png"));
        assetName = AsciiSpritesEditor.GetAssetName('3', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-110011-63-51-33.png"));
        assetName = AsciiSpritesEditor.GetAssetName('A', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-1000001-101-65-41.png"));
        assetName = AsciiSpritesEditor.GetAssetName('a', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-1100001-141-97-61.png"));
        assetName = AsciiSpritesEditor.GetAssetName('~', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-1111110-176-126-7e.png"));

        // Long form
        templateFileName = "char-{binary}-{octal}-{decimal}-{hexadecimal}.png";
        assetName = AsciiSpritesEditor.GetAssetName(' ', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-100000-40-32-20.png"));
        assetName = AsciiSpritesEditor.GetAssetName('3', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-110011-63-51-33.png"));
        assetName = AsciiSpritesEditor.GetAssetName('A', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-1000001-101-65-41.png"));
        assetName = AsciiSpritesEditor.GetAssetName('a', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-1100001-141-97-61.png"));
        assetName = AsciiSpritesEditor.GetAssetName('~', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-1111110-176-126-7e.png"));

        // Numeric form
        templateFileName = "char-{2}-{8}-{10}-{16}.png";
        assetName = AsciiSpritesEditor.GetAssetName(' ', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-100000-40-32-20.png"));
        assetName = AsciiSpritesEditor.GetAssetName('3', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-110011-63-51-33.png"));
        assetName = AsciiSpritesEditor.GetAssetName('A', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-1000001-101-65-41.png"));
        assetName = AsciiSpritesEditor.GetAssetName('a', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-1100001-141-97-61.png"));
        assetName = AsciiSpritesEditor.GetAssetName('~', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-1111110-176-126-7e.png"));
    }

    [Test]
    public void ExpandsMultipleOfSameTemplateString()
    {
        string assetName;
        string templateFileName;

        // Short octal
        templateFileName = "char-{oct}-{oct}.png";
        assetName = AsciiSpritesEditor.GetAssetName(' ', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-40-40.png"));
        assetName = AsciiSpritesEditor.GetAssetName('3', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-63-63.png"));
        assetName = AsciiSpritesEditor.GetAssetName('A', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-101-101.png"));
        assetName = AsciiSpritesEditor.GetAssetName('a', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-141-141.png"));
        assetName = AsciiSpritesEditor.GetAssetName('~', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-176-176.png"));

        // Long decimal
        templateFileName = "char-{decimal}-{decimal}.png";
        assetName = AsciiSpritesEditor.GetAssetName(' ', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-32-32.png"));
        assetName = AsciiSpritesEditor.GetAssetName('3', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-51-51.png"));
        assetName = AsciiSpritesEditor.GetAssetName('A', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-65-65.png"));
        assetName = AsciiSpritesEditor.GetAssetName('a', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-97-97.png"));
        assetName = AsciiSpritesEditor.GetAssetName('~', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-126-126.png"));

        // Numeric hexadecimal
        templateFileName = "char-{16}-{16}.png";
        assetName = AsciiSpritesEditor.GetAssetName(' ', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-20-20.png"));
        assetName = AsciiSpritesEditor.GetAssetName('3', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-33-33.png"));
        assetName = AsciiSpritesEditor.GetAssetName('A', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-41-41.png"));
        assetName = AsciiSpritesEditor.GetAssetName('a', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-61-61.png"));
        assetName = AsciiSpritesEditor.GetAssetName('~', templateFileName);
        Assert.That(assetName, Is.EqualTo("Assets/char-7e-7e.png"));
    }

    [Test]
    public void ExpandsTemplateStringIgnoringCase()
    {
        string assetName;
        string expectedAssetName;

        // Short decimal form
        expectedAssetName = "Assets/char-32.png";
        assetName = AsciiSpritesEditor.GetAssetName(' ', "char-{dec}.png");
        Assert.That(assetName, Is.EqualTo(expectedAssetName));
        assetName = AsciiSpritesEditor.GetAssetName(' ', "char-{deC}.png");
        Assert.That(assetName, Is.EqualTo(expectedAssetName));
        assetName = AsciiSpritesEditor.GetAssetName(' ', "char-{DEc}.png");
        Assert.That(assetName, Is.EqualTo(expectedAssetName));
        assetName = AsciiSpritesEditor.GetAssetName(' ', "char-{DEC}.png");
        Assert.That(assetName, Is.EqualTo(expectedAssetName));

        // Long octal form
        expectedAssetName = "Assets/char-40.png";
        assetName = AsciiSpritesEditor.GetAssetName(' ', "char-{octal}.png");
        Assert.That(assetName, Is.EqualTo(expectedAssetName));
        assetName = AsciiSpritesEditor.GetAssetName(' ', "char-{ocTAl}.png");
        Assert.That(assetName, Is.EqualTo(expectedAssetName));
        assetName = AsciiSpritesEditor.GetAssetName(' ', "char-{oCTAl}.png");
        Assert.That(assetName, Is.EqualTo(expectedAssetName));
        assetName = AsciiSpritesEditor.GetAssetName(' ', "char-{OCTAL}.png");
        Assert.That(assetName, Is.EqualTo(expectedAssetName));
    }

    [Test]
    public void DoesNotExpandMissingTemplateString()
    {
        string assetName = AsciiSpritesEditor.GetAssetName(' ', "derp.png");
        Assert.That(assetName, Is.EqualTo("Assets/derp.png"));
    }

}
