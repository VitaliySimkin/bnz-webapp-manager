<template>
<div v-on:keyup.enter="onEnterKeyup">
	<div class="site-console-panel" style="padding: 6px 6px 0 6px;">
		<div class="left-console-panel">
			<el-button type="primary" size="small" @click="executeSQL" :loading="sqlExecuting" :disabled="!sql">Execute</el-button>
			<el-dropdown @command="applyTemplate">
				<el-button type="primary" size="small">
					Шаблони<i class="el-icon-arrow-down el-icon--right"></i>
				</el-button>
				<el-dropdown-menu slot="dropdown">
					<el-dropdown-item v-for="(item, index) in templates" :key="index" :command="item"
						>{{item.caption}}</el-dropdown-item>
				</el-dropdown-menu>
			</el-dropdown>
		</div>
		<div class="right-cnt">
			<el-button v-if="result" icon="el-icon-download" size="small" @click="loadCSV">.csv</el-button>
			<span v-if="result" style="font-family: monospace;font-size: 15px;color: #555;
				padding-right: 10px;">{{sqlExecutionState}}</span>
			
		</div>
	</div>
	<div class="site-console-content">
		<codemirror v-model="sql" :options="sqlOptions" style="margin: 5px 0px;max-height: 500px;overflow: auto;border-top: solid 1px #ccc;border-bottom: solid 1px #ccc;"></codemirror>
		<div v-if="result" style="padding:5px" >
			<el-table v-if="result.success" :data="result.rows" size="small" max-height="600" border>
				<el-table-column show-overflow-tooltip v-for="(column, columnIndex) in result.columns"
					:label="column" :key=(columnIndex) :prop="String(columnIndex)"></el-table-column>
			</el-table>
			<div v-else class="sql-error-cnt">
				<span class="sql-error-message">{{result.errorMessage}}</span>
				<span class="sql-error-stack">{{result.errorStack}}</span>
			</div>
		</div>
	</div>
</div>
</template>

<script lang="ts">
import Vue from "vue";
import store from "../store/index";
import { WebApp, ApplicationPoolData, DBApi, SQLQueryResult,
	RedisApi, ApplicationPoolApi, StringStringKeyValuePair } from "../../../api-client/index";
import { Component, Prop, Model, Watch } from "vue-property-decorator";
import { State } from "vuex-class";

import QueryTemplateManager from "./QueryTemplateManager";

// tslint:disable-next-line
const VueCodemirror = require("vue-codemirror");
import "codemirror/lib/codemirror.css";
import "codemirror/mode/sql/sql.js";
import "cm-show-invisibles";
import SQLQuery from "@/components/SQLQuery";
Vue.use(VueCodemirror);


@Component
export default class SqlQueryPanel extends Vue {

	@Prop({required: true})
	public app: WebApp;

	@Prop({required: true})
	public query: SQLQuery;

	public sql: string = "";

	public get result(): SQLQueryResult | null {
		return this.query.result;
	}

	public set result(value) {
		this.query.result = value;
	}

	public get templates() {
		return QueryTemplateManager.getTemplatesFor(this.app.sqldbType);
	}

	public sqlExecuting: boolean = false;

	protected sqlOptions: object = {
		tabSize: 4,
		mode: "text/x-mssql",
		lineNumbers: true,
		indentWithTabs: true,
		showInvisibles: true,
		line: true,
		viewportMargin: Infinity,
	};

	public applyTemplate(item: {sql: string}) {
		this.sql = item.sql;
	}


	@Watch("sql")
	public handleInput() {
		this.query.sql = this.sql;
		this.$emit("onQuerySQLChange");
	}

	@Watch("query")
	public onChange_sqlQuery() {
		if (this.sql !== this.query.sql) {
			this.sql = this.query.sql;
		}
	}

	public mounted() {
		this.onChange_sqlQuery();
	}

	public onEnterKeyup(event: KeyboardEvent) {
		if (event.ctrlKey) {
			event.preventDefault();
			event.stopPropagation();
			this.executeSQL();
		}
	}

	protected async executeSQL() {
		this.sqlExecuting = true;
		this.result = null;
		this.result = await new DBApi().execute(this.app.id, { sql: this.sql });
		this.sqlExecuting = false;
	}

	protected get sqlExecutionState() {
		if (this.sqlExecuting) {
			return "running...";
		} else if (this.result) {
			return `Time: ${this.result!.executeTime} ms`;
		} else {
			return "";
		}
	}

	protected loadCSV() {
		const columns = this.result!.columns || [];
		const data = this.result!.rows || [];
		const csvContent = "data:text/csv;charset=utf-8," +
			[columns].concat(data).map((item) => item.join(",")).join("\n");
		const encodedUri = encodeURI(csvContent);
		const link = document.createElement("a");
		link.setAttribute("href", encodedUri);
		const siteName = this.app.path.replace(new RegExp("[^a-z0-9]", "gi"), "");
		const fileName = `${siteName}_sqlresult.csv`;
		link.setAttribute("download", fileName);
		document.body.appendChild(link);
		link.click();
		link.remove();
	}

}

</script>

<style lang="less">

.site-console-panel {
    display: flex;
    justify-content: space-between;
    align-items: baseline;
}

div.sql-error-cnt {
    padding: 5px 10px;
    border: solid 1px #aaa;
}
span.sql-error-message {
    font-family: monospace;
    font-size: 17px;
    display: block;
    color: #ff0000;
    font-weight: bold;
}
span.sql-error-stack {
    font-family: monospace;
    white-space: break-spaces;
    display: block;
    color: #c30000;
}

      .CodeMirror {
        border: 1px solid #eee;
        height: auto;
      }
      .cm-tab {
         background: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAADAAAAAMCAYAAAAkuj5RAAAAAXNSR0IArs4c6QAAAGFJREFUSMft1LsRQFAQheHPowAKoACx3IgEKtaEHujDjORSgWTH/ZOdnZOcM/sgk/kFFWY0qV8foQwS4MKBCS3qR6ixBJvElOobYAtivseIE120FaowJPN75GMu8j/LfMwNjh4HUpwg4LUAAAAASUVORK5CYII=);
         background-position: right;
         background-repeat: no-repeat;
      }
.application-processes-description {
	font-family: monospace;
}



body {
    padding: 0;
    margin: 0
}

.app {
    padding-top: 40px
}

.header-panel {
    position: fixed;
    width: 100%;
    top: 0;
    z-index: 2000;
    height: 40px;
    border-bottom: solid 1px #eee;
    box-shadow: 0 0 5px 0px #aaa;
}

.header-panel .el-input__inner {
    border-radius: 0;
    border-bottom: none;
    border-top: none;
}

.apps-list-cnt {
    padding: 10px;
    background-color: #fff;
    box-sizing: border-box;
    height: calc(100vh - 40px);
    overflow: auto
}

.web-app-cnt {
    padding: 5px 10px;
    margin-bottom: 10px;
    width: 100%;
    display: block;
    border: solid 1px #eee;
    box-shadow: 0 0 10px -5px #777;
    box-sizing: border-box
}

.web-app-cnt:hover {
    box-shadow: 0 0 10px -5px red;
    border-color: #11d5ff33;
    background-color: #ff000007
}

.app-control-panel {
    display: flex;
    flex-direction: column;
    justify-content: space-between
}

@media only screen and (min-width: 700px) {
    .app-control-panel {
        flex-direction:row
    }
}

.app-wrap-cnt {
    margin-top: 7px;
    display: block;
    padding: 5px;
    border: solid 1px #ccc;
    border-radius: 4px;
    background-color: #fff
}

.app-console-panel {
    margin-bottom: 7px
}

.code-editor-cnt {
    border: solid 1px #ddd
}

.app-caption {
    font-size: 28px;
    font-family: monospace
}

.app-property-name {
    font-family: sans-serif;
    font-weight: 700
}

.app-property-value {
    font-family: monospace
}

.app-property-value .cell {
    word-break: break-word
}

::-webkit-scrollbar {
    width: 10px;
    height: 10px
}

::-webkit-scrollbar-track {
    background: #f1f1f1!important
}

::-webkit-scrollbar-thumb {
    background: #bbb!important
}

::-webkit-scrollbar-thumb:hover {
    background: #888!important
}

.right-cnt > * {
    margin-left: 12px;
}

.left-console-panel > *:not(:last-child) {
	margin-right: 5px;
}

</style>
