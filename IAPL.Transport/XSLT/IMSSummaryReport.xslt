<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:param name="IMSDataStatusReportDate" />
	<xsl:param name="Country" />
	<xsl:param name="CountryCode" />
	<xsl:param name="IMSDataSeq" />
	<xsl:param name="SizeKB" />
	<xsl:param name="ISGDateSent" />
	<xsl:param name="Issue" />
	<xsl:param name="CommentResolution" />
	<xsl:param name="DateFileRcvd" />
	<xsl:param name="TransactionDate" />
	<xsl:param name="RecordCount" />
	<xsl:param name="IMSExtractionValue" />
	<xsl:template match="/NewDataSet">
		<html>
			<head>
				<title>IMS Summary Report</title>
			</head>
			<body>
				<table border="1">
					<tr align="right">
						<td>
							<img src="http://www.gsk.com/common/img/logo-gsk.gif" />
							<br />
							<br />
						</td>
					</tr>
					<tr>
						<td>Dear All,</td>
					</tr>
					<tr>
						<td>
							Good Afternoon.<br /><br />
						</td>
					</tr>
					<tr>
						<td colspan="3">
							<font size="5" color="black">IMS Data Status Report - <xsl:value-of select="IMSDataStatusReportDate" /></font>
						</td>
					</tr>
				</table>
				<br />
				<table border="1" cellpadding="0" cellspacing="0">
					<tr>
						<td align="center" width="200" bgcolor="#663300">
							<font size="3" color="#ffffff" style="font-size: 10px; font-family: Arial;">COUNTRY</font>
						</td>
						<td align="center" width="200" bgcolor="#663300">
							<font size="3" color="#ffffff" style="font-size: 10px; font-family: Arial;">IMS DATA SEQUENCE</font>
						</td>
						<td align="center" width="200" bgcolor="#663300">
							<font size="3" color="#ffffff" style="font-size: 10px; font-family: Arial;">SIZE KB</font>
						</td>
						<td align="center" width="200" bgcolor="#663300">
							<font size="3" color="#ffffff" style="font-size: 10px; font-family: Arial;">ISG DATE SENT</font>
						</td>
						<td align="center" width="200" bgcolor="#663300">
							<font size="3" color="#ffffff" style="font-size: 10px; font-family: Arial;">ISSUE</font>
						</td>
						<td align="center" width="200" bgcolor="#663300">
							<font size="3" color="#ffffff" style="font-size: 10px; font-family: Arial;">COMMENT / RESOLUTION</font>
						</td>
						<td align="center" width="200" bgcolor="#663300">
							<table border="0" cellpadding="0" cellspacing="0">
								<tr>
									<td align="center" >
										<font size="3" color="#ffffff" style="font-size: 10px; font-family: Arial;">DATA FILE RECEIVED</font>
									</td>
								</tr>
								<tr>
									<td align="center" bgcolor="#ffff99">
										<font size="3" color="#000000" style="font-size: 10px; font-family: Arial;">ISG verified through IMS server access</font>
									</td>
								</tr>
							</table>

						</td>
						<td align="center" width="300" bgcolor="#ffff66">
							<font size="3" color="#000000" style="font-size: 10px; font-family: Arial;">
								TRANSACTION DATE <br />(Checked by ISG)
							</font>
						</td>
						<td align="center" width="200" bgcolor="#663300">
							<font size="3" color="#ffffff" style="font-size: 10px; font-family: Arial;">RECORD COUNT</font>
						</td>
						<td align="center" width="200" bgcolor="#663300">
							<font size="3" color="#ffffff" style="font-size: 10px; font-family: Arial;">IMS EXTRACTION VALUE</font>
						</td>
					</tr>
					
					<xsl:for-each select="Details">
					<tr>
						<td align="center" bgcolor="#ffff66" width="200">
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
								<xsl:when test="Issue = ''">&#160;</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="Issue" />
								</xsl:otherwise>
							</xsl:choose>
						</td>
						<td align="center">
							<xsl:choose>
								<xsl:when test="CommentResolution = ''">&#160;</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="CommentResolution" />
								</xsl:otherwise>
							</xsl:choose>
						</td>
						<td align="center">
							<xsl:choose>
								<xsl:when test="DataFileRcvd = ''">&#160;</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="DataFileRcvd" />
								</xsl:otherwise>
							</xsl:choose>
						</td>
						<td align="center">
							<xsl:choose>
								<xsl:when test="TransactionDate = ''">&#160;</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="TransactionDate" />
								</xsl:otherwise>
							</xsl:choose>
						</td>
						<td align="center">
							<xsl:choose>
								<xsl:when test="RecordCount = ''">&#160;</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="RecordCount" />
								</xsl:otherwise>
							</xsl:choose>
						</td>
						<td align="center">
							<xsl:choose>
								<xsl:when test="IMSExtractionValue = ''">&#160;</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="IMSExtractionValue" />
								</xsl:otherwise>
							</xsl:choose>
						</td>
					</tr>
					</xsl:for-each >
					
				</table>
				<br />
				<br />
				<table>
					<tr>
						<td>Kindly immediately report any issues or concerns.</td>
					</tr>
					<tr>
						<td>Thank you very much.</td>
					</tr>
				</table>
			</body>
		</html>		
	</xsl:template>
</xsl:stylesheet>
