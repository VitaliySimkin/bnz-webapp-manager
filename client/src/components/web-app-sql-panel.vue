<template>
<div class="app-wrap-cnt" style="padding: 0" :fullscreen="isFullScreen">
	<div class="site-console-panel web-app-sql-main-panel">
		<span class="sql-db-type">{{ isFullScreen ? app.path + " " : "" }}{{app.sqldbType}}</span>
		<!-- <el-button size="small" @click="addQuery" icon="el-icon-plus"></el-button> -->
		<el-button size="small" @click="isFullScreen = !isFullScreen" icon="el-icon-full-screen"></el-button>
	</div>
	<div class="site-console-content">
		<sql-query-panel :app="app" v-model="sqlQuery"></sql-query-panel>
		<!-- <el-tabs v-model="activeName" type="card" closable>
			<el-tab-pane v-for="item in queries" :key="item.title" :label="item.title" :name="item.title">
				<sql-query-panel :app="app" :query="item.sqlQuery"></sql-query-panel>
			</el-tab-pane>
		</el-tabs> -->
	</div>
</div>
</template>

<script lang="ts">
import Vue from "vue";
import store from "../store/index";
import { Component, Prop, Watch } from "vue-property-decorator";
import { WebApp, ApplicationPoolData, DBApi, SQLQueryResult,
	RedisApi, ApplicationPoolApi, StringStringKeyValuePair } from "../../../api-client/index";
import SqlQueryPanel from "./sql-query-panel.vue";


@Component({
	components: {
		SqlQueryPanel,
	},
})
export default class WebAppSqlPanel extends Vue {

	@Prop({required: true})
	public app: WebApp;

	public activeName: string | null = null;

	public sqlQuery: string = "SELECT 1";

	public queries: Array<{ title: string, sqlQuery: string }> = [];

	public isFullScreen: boolean = false;

	public addQuery() {
		this.queries.push({
			title: "Query " + +new Date(),
			sqlQuery: "SELECT 1",
		});
	}

	public saveDataToCache() {
		const key = `${this.app.id}_sqlDataCache`;
		localStorage.setItem(key, JSON.stringify({
			isFullScreen: this.isFullScreen,
			sqlQuery: this.sqlQuery,
		}));
	}

	public loadDataFromCache() {
		const key = `${this.app.id}_sqlDataCache`;
		try {
			const storageItem = localStorage.getItem(key);
			const data = JSON.parse(storageItem || "") as {sqlQuery: string, isFullScreen: boolean };
			this.isFullScreen = data.isFullScreen;
			this.sqlQuery = data.sqlQuery;
		} catch {
			this.activeName = "SELECT 1";
		}
	}

	public created() {
		this.loadDataFromCache();
	}

	@Watch("isFullScreen")
	protected onChange_isFullScreen() {
		this.saveDataToCache();
	}
	@Watch("sqlQuery")
	protected onChange_sqlQuery() {
		this.saveDataToCache();
	}

}

</script>

<style lang="less">

[fullscreen] {
    position: absolute;
    width: 100vw;
    height: 100vh;
    top: 0;
    left: 0;
    background-color: #fff !important;
    z-index: 10000000;
    margin: 0 !important;
    border: none !important;
    box-sizing: border-box;
	padding: 0 !important;
	
	.web-app-sql-main-panel {
		padding: 5px;
		border-bottom: solid 1px #eee;
		box-shadow: 0 0 5px 0px #aaa;
	}

}

.sql-db-type {
    font-family: monospace;
    font-weight: bold;
    font-size: 18px;
    margin: 0 10px;
    color: #777;
}

.web-app-sql-main-panel {
	padding: 5px;
    border-bottom: solid 2px #eee;
}

</style>
