<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:output method="html"/>

  <xsl:preserve-space elements="p code var i b"/>

  <xsl:template match="doc">
    <html>
    <head><title><xsl:value-of select="maintitle"/></title></head>
    <body>
    <h1><xsl:value-of select="maintitle"/></h1>
    <h2>Table of Contents</h2>
    <ul>
    <xsl:apply-templates select="section" mode="toc"/>
    </ul>

    <xsl:apply-templates/>
    </body>
    </html>
  </xsl:template>

  <xsl:template match="maintitle">
  </xsl:template>

  <xsl:template name="section_header_text">
    <xsl:choose>
      <xsl:when test="@name">
        <a><xsl:attribute name="name"><xsl:value-of select="@name"/></xsl:attribute><xsl:value-of select="title/text()"/></a>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="title/text()"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="section">
    <xsl:choose>
      <xsl:when test="ancestor::section">
        <h3>
          <xsl:call-template name="section_header_text"/>
        </h3>
      </xsl:when>
      <xsl:otherwise>
        <h2>
          <xsl:call-template name="section_header_text"/>
        </h2>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:apply-templates select="*[name()!='title']"/>
  </xsl:template>

  <xsl:template match="section" mode="toc">
    <li>
      <xsl:choose>
        <xsl:when test="@name">
          <a><xsl:attribute name="href">#<xsl:value-of select="@name"/></xsl:attribute><xsl:value-of select="title/text()"/></a>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="title/text()"/>
        </xsl:otherwise>
      </xsl:choose>
    </li>
    <xsl:if test="section">
      <ul>
      <xsl:apply-templates select="section" mode="toc"/>
      </ul>
    </xsl:if>
  </xsl:template>

  <xsl:template match="p">
    <p><xsl:apply-templates/></p>
  </xsl:template>

  <xsl:template match="code">
    <xsl:for-each select="child::node()">
      <xsl:choose>
        <xsl:when test="self::var">
          <font color="#FF0000"><i><xsl:apply-templates/></i></font>
        </xsl:when>
        <xsl:when test="self::text()">
          <font color="#0000FF"><code><xsl:value-of select="."/></code></font>
        </xsl:when>
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>

  <xsl:template match="var">
    <xsl:choose>
      <xsl:when test="ancestor::code">
        <font color="#FF0000" face="Times" size="3"><i><xsl:apply-templates/></i></font>
      </xsl:when>
      <xsl:otherwise>
        <font color="#FF0000"><i><xsl:apply-templates/></i></font>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="b">
    <b><xsl:apply-templates/></b>
  </xsl:template>

  <xsl:template match="i">
    <i><xsl:apply-templates/></i>
  </xsl:template>

  <xsl:template match="xref">
    <xsl:variable name="dest"><xsl:value-of select="@dest"/></xsl:variable>
    <a><xsl:attribute name="href">#<xsl:value-of select="@dest"/></xsl:attribute><xsl:value-of select="//section[@name=$dest]/title/text()"/></a>
  </xsl:template>

  <xsl:template match="link">
    <a><xsl:attribute name="href"><xsl:value-of select="@url"/></xsl:attribute><xsl:value-of select="."/></a>
  </xsl:template>

  <xsl:template match="ul">
    <ul>
    <xsl:apply-templates mode="ul"/>
    </ul>
  </xsl:template>

  <xsl:template match="ulp">
    <ul>
    <xsl:apply-templates mode="ulp"/>
    </ul>
  </xsl:template>

  <xsl:template match="li" mode="ul">
    <li><xsl:apply-templates/></li>
  </xsl:template>

  <xsl:template match="ul" mode="ul">
    <ul>
    <xsl:apply-templates mode="ul"/>
    </ul>
  </xsl:template>

  <xsl:template match="li" mode="ulp">
    <li><p><xsl:apply-templates/></p></li>
  </xsl:template>

  <xsl:template match="ul" mode="ulp">
    <ul>
    <xsl:apply-templates mode="ul"/>
    </ul>
  </xsl:template>

  <xsl:template match="ulp" mode="ulp">
    <ul>
    <xsl:apply-templates mode="ulp"/>
    </ul>
  </xsl:template>

</xsl:stylesheet>
