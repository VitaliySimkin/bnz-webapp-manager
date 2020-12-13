import { SQLQueryResult } from "../../../api-client/index";

export default class SQLQuery {

	public key: string;

	public sql: string;

	public caption: string;

	public result: SQLQueryResult | null;

	constructor(defaultValues?: {key: string, sql: string, caption: string}) {
		this.key = defaultValues?.key || this.makeId(10);
		this.sql = defaultValues?.sql || "";
		this.caption = defaultValues?.caption || "Query " + +new Date();
		this.result = null;
	}

	public toJSON() {
		return {
			key: this.key,
			sql: this.sql,
			caption: this.caption,
		};
	}


	private makeId(length: number) {
		let result = "";
		const characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
		const charactersLength = characters.length;
		for ( let i = 0; i < length; i++ ) {
			result += characters.charAt(Math.floor(Math.random() * charactersLength));
		}
		return result;
	}

}
