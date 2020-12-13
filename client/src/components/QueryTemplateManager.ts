enum DBType {
	Oracle = "Oracle",
	MSSQL = "MSSQL",
	PostgreSQL = "PostgreSQL",
}

const templates = [
	{
		caption: "SELECT SysSettings",
		sql: {
			[DBType.Oracle]: `SELECT
					"ss"."Code"
					,"ss"."Name"
					,"ss"."ValueTypeName"
					,"ssv"."TextValue"
					,"ssv"."IntegerValue"
					,"ssv"."FloatValue"
					,"ssv"."BooleanValue"
					,"ssv"."DateTimeValue"
					,"ssv"."GuidValue"
					,"ssv"."BinaryValue"
				FROM "SysSettings" "ss"
				INNER JOIN "SysSettingsValue" "ssv" ON "ssv"."SysSettingsId" = "ss"."Id"
				WHERE "ss"."Code" LIKE '%%'`.replaceAll("\n				", "\n"),
			[DBType.MSSQL]: `SELECT
					[ss].[Code]
					,[ss].[Name]
					,[ss].[ValueTypeName]
					,[ssv].[TextValue]
					,[ssv].[IntegerValue]
					,[ssv].[FloatValue]
					,[ssv].[BooleanValue]
					,[ssv].[DateTimeValue]
					,[ssv].[GuidValue]
					,[ssv].[BinaryValue]
				FROM [dbo].[SysSettings] [ss] WITH(NOLOCK)
				INNER JOIN [dbo].[SysSettingsValue] [ssv] WITH(NOLOCK) ON [ssv].[SysSettingsId] = [ss].[Id]
				WHERE [ss].[Code] LIKE '%%';`.replaceAll("\n				", "\n"),
			[DBType.PostgreSQL]: `SELECT
					"ss"."Code"
					,"ss"."Name"
					,"ss"."ValueTypeName"
					,"ssv"."TextValue"
					,"ssv"."IntegerValue"
					,"ssv"."FloatValue"
					,"ssv"."BooleanValue"
					,"ssv"."DateTimeValue"
					,"ssv"."GuidValue"
					,"ssv"."BinaryValue"
				FROM "SysSettings" "ss"
				INNER JOIN "SysSettingsValue" "ssv" ON "ssv"."SysSettingsId" = "ss"."Id"
				WHERE "ss"."Code" LIKE '%%';`.replaceAll("\n				", "\n"),
		},
	},
	{
		caption: "SELECT SysSettings (more info)",
		sql: {
			[DBType.Oracle]: `SELECT
					"ss"."Code"
					,"ss"."Name"
					,"ss"."ValueTypeName"
					,"schema"."Name" AS "LookupName"
					,"ssv"."TextValue"
					,"ssv"."IntegerValue"
					,"ssv"."FloatValue"
					,"ssv"."BooleanValue"
					,"ssv"."DateTimeValue"
					,"ssv"."GuidValue"
					,"ssv"."BinaryValue"
					,"ss"."IsPersonal"
					,"ss"."IsCacheable"
					,"sau"."Name" AS "SysAdminUnit"
				FROM "SysSettings" "ss"
				INNER JOIN "SysSettingsValue" "ssv" ON "ssv"."SysSettingsId" = "ss"."Id"
				LEFT JOIN "SysAdminUnit" "sau" ON "sau"."Id" = "ssv"."SysAdminUnitId"
				LEFT JOIN "SysSchema" "schema" ON "schema"."UId" = "ss"."ReferenceSchemaUId"
				WHERE "ss"."Code" LIKE '%%'`.replaceAll("\n				", "\n"),
			[DBType.MSSQL]: `SELECT
					[ss].[Code]
					,[ss].[Name]
					,[ss].[ValueTypeName]
					,[schema].[Name] AS [LookupName]
					,[ssv].[TextValue]
					,[ssv].[IntegerValue]
					,[ssv].[FloatValue]
					,[ssv].[BooleanValue]
					,[ssv].[DateTimeValue]
					,[ssv].[GuidValue]
					,[ssv].[BinaryValue]
					,[ss].[IsPersonal]
					,[ss].[IsCacheable]
					,[sau].[Name] AS [SysAdminUnit]
				FROM [dbo].[SysSettings] [ss] WITH(NOLOCK)
				INNER JOIN [dbo].[SysSettingsValue] [ssv] WITH(NOLOCK) ON [ssv].[SysSettingsId] = [ss].[Id]
				LEFT JOIN [dbo].[SysAdminUnit] [sau] WITH(NOLOCK) ON [sau].[Id] = [ssv].[SysAdminUnitId]
				LEFT JOIN [dbo].[SysSchema] [schema] WITH(NOLOCK) ON [schema].[UId] = [ss].[ReferenceSchemaUId]
				WHERE [ss].[Code] LIKE '%%';`.replaceAll("\n				", "\n"),
			[DBType.PostgreSQL]: `SELECT
					"ss"."Code"
					,"ss"."Name"
					,"ss"."ValueTypeName"
					,"schema"."Name" AS "LookupName"
					,"ssv"."TextValue"
					,"ssv"."IntegerValue"
					,"ssv"."FloatValue"
					,"ssv"."BooleanValue"
					,"ssv"."DateTimeValue"
					,"ssv"."GuidValue"
					,"ssv"."BinaryValue"
					,"ss"."IsPersonal"
					,"ss"."IsCacheable"
					,"sau"."Name" AS "SysAdminUnit"
				FROM "SysSettings" "ss"
				INNER JOIN "SysSettingsValue" "ssv" ON "ssv"."SysSettingsId" = "ss"."Id"
				LEFT JOIN "SysAdminUnit" "sau" ON "sau"."Id" = "ssv"."SysAdminUnitId"
				LEFT JOIN "SysSchema" "schema" ON "schema"."UId" = "ss"."ReferenceSchemaUId"
				WHERE "ss"."Code" LIKE '%%';`.replaceAll("\n				", "\n"),
		},
	},
];

export default {
	getTemplatesFor(dbTypeName: string) {
		const dbType = DBType[dbTypeName as keyof typeof DBType];
		return templates
			.filter((item) => item.sql[dbType])
			.map((item) => ({caption: item.caption, sql: item.sql[dbType]}));
	},
};
