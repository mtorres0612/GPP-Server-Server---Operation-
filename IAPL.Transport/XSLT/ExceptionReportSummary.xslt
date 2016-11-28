<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:param name="IMSDataStatusReportDate" />
	<xsl:param name="Country" />
	<xsl:param name="CountryCode" />
	<xsl:param name="IMSDataSeq" />
	<xsl:param name="SizeKB" />
	<xsl:param name="ISGDateSent" />
	<xsl:param name="SendStatus" />
	<xsl:param name="Issue" />
	<xsl:param name="Resolution" />
	<xsl:param name="IMSSingaporeComment" />
	<xsl:param name="ISGComment" />

	<xsl:template match="/NewDataSet">
		<html>
			<head>
				<title>Exception Report Summary</title>
			</head>
			<body>
				<table>
					<tr>
						<td colspan="3">
							<font size="5" color="black">Exception Report Summary - <xsl:value-of select="IMSDataStatusReportDate" />
						</font>
						</td>
					</tr>
				</table>
				<br />
				<table border="1" cellpadding="0" cellspacing="0">
					<tr>
						<td border="1" align="center" width="200" height="21px" bgcolor="#663300">
							<font size="3" color="#ffffff" style="font-size: 10px; font-family: Arial;">COUNTRY</font>
						</td>
						<td border="1" align="center" width="200" height="21px" bgcolor="#663300">
							<font size="3" color="#ffffff" style="font-size: 10px; font-family: Arial;">IMS DATA SEQUENCE</font>
						</td>
						<td border="1" align="center" width="200" height="21px" bgcolor="#663300">
							<font size="3" color="#ffffff" style="font-size: 10px; font-family: Arial;">SIZE KB</font>
						</td>
						<td border="1" align="center" width="200" height="21px" bgcolor="#663300">
							<font size="3" color="#ffffff" style="font-size: 10px; font-family: Arial;">ISG DATE SENT</font>
						</td>
						<td border="1" align="center" width="200" height="21px" bgcolor="#663300">
							<font size="3" color="#ffffff" style="font-size: 10px; font-family: Arial;">SEND STATUS</font>
						</td>
						<td border="1" align="center" width="200" height="21px" bgcolor="#663300">
							<font size="3" color="#ffffff" style="font-size: 10px; font-family: Arial;">ISSUE</font>
						</td>
						<td border="1" align="center" width="200" height="21px" bgcolor="#663300">
							<font size="3" color="#ffffff" style="font-size: 10px; font-family: Arial;">RESOLUTION</font>
						</td>
						<td border="1" align="center" width="200" height="21px" bgcolor="#663300">
							<font size="3" color="#ffffff" style="font-size: 10px; font-family: Arial;">IMS-SINGAPORE COMMENT</font>
						</td>
						<td border="1" align="center" width="200" height="21px" bgcolor="#663300">
							<font size="3" color="#ffffff" style="font-size: 10px; font-family: Arial;">ISG COMMENT</font>
						</td>
					</tr>

					<xsl:for-each select="Details">
					<tr>
						<td border="1" align="center" bgcolor="#ffff66">
							<font size="3" color="#000000">
								<xsl:value-of select="Country" />(<xsl:value-of select="CountryCode" />)
							</font>
						</td>
						
						<td align="center">
							<xsl:choose>
								<xsl:when test="IMSDataSeq = ''">&#160;</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="IMSDataSeq" />
								</xsl:otherwise>
							</xsl:choose>
						</td>
						<td align="center">
							<xsl:choose>
								<xsl:when test="SizeKB = ''">&#160;</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="SizeKB" />
								</xsl:otherwise>
							</xsl:choose>
						</td>
						<td align="center">
							<xsl:choose>
								<xsl:when test="ISGDateSent = ''">&#160;</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="ISGDateSent" />
								</xsl:otherwise>
							</xsl:choose>
						</td>
						<td align="center">
							<xsl:choose>
								<xsl:when test="SendStatus = ''">&#160;</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="SendStatus" />
								</xsl:otherwise>
							</xsl:choose>
						</td>
						<td align="center">
							<xsl:choose>
								<xsl:when test="Issue = ''">&#160;</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="Issue" />
								</xsl:otherwise>
							</xsl:choose>
						</td>
						<td align="center">
							<xsl:choose>
								<xsl:when test="Resolution = ''">&#160;</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="Resolution" />
								</xsl:otherwise>
							</xsl:choose>
						</td>
						<td align="center">
							<xsl:choose>
								<xsl:when test="IMSSingaporeComment = ''">&#160;</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="IMSSingaporeComment" />
								</xsl:otherwise>
							</xsl:choose>
						</td>
						<td align="center">
							<xsl:choose>
								<xsl:when test="ISGComment = ''">&#160;</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="ISGComment" />
								</xsl:otherwise>
							</xsl:choose>
						</td>
					</tr>
					</xsl:for-each >
					
				</table>
			</body>
		</html>		
	</xsl:template>
</xsl:stylesheet>