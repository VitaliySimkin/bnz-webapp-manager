<template>
<div v-on:keyup.enter="onEnterKeyup">
	<div class="site-console-panel" style="padding: 6px 6px 0 6px;">
		<el-button type="primary" size="small" @click="executeSQL" :loading="sqlExecuting">Execute</el-button>
		<div class="right-cnt">
			<span v-if="result" style="font-family: monospace;font-size: 15px;color: #555;
				padding-right: 10px;">{{sqlExecutionState}}</span>
		</div>
	</div>
	<div class="site-console-content">
		<codemirror v-model="query" :options="sqlOptions" style="margin: 5px 0;"></codemirror>
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

// tslint:disable-next-line
const VueCodemirror = require("vue-codemirror");
import "codemirror/lib/codemirror.css";
import "codemirror/mode/sql/sql.js";
import "cm-show-invisibles";
Vue.use(VueCodemirror);


@Component
export default class SqlQueryPanel extends Vue {

	@Prop({required: true})
	public app: WebApp;

	@Model("input", {required: true})
	public sqlQuery: string;

	public query: string = "";

	public result: SQLQueryResult | null = null;

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


	@Watch("query")
	public handleInput() {
		this.$emit("input", this.query);
	}

	@Watch("sqlQuery")
	public onChange_sqlQuery() {
		if (this.query !== this.sqlQuery) {
			this.query = this.sqlQuery;
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
		this.result = await new DBApi().execute(this.app.id, { sql: this.query });
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
    width: 10px!important;
    height: 10px!important
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

</style>
