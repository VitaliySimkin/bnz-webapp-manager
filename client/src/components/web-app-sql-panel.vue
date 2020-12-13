<template>
<div class="app-wrap-cnt" style="padding: 0" :fullscreen="isFullScreen">
	<div class="site-console-panel web-app-sql-main-panel">
		<span class="sql-db-type"><a v-show="isFullScreen" :href="app.path">{{app.path}}</a>{{ isFullScreen ? " " : "" }}{{app.sqldbType}}</span>
		<el-button size="small" class="add-tab-button" @click="addQuery" icon="el-icon-plus"></el-button>
		<div class="tab-panel">
			<div v-for="queryItem in queries" :key="queryItem.key" class="tab-header-item"
				@click="activeQueryKey = queryItem.key"
				:active="queryItem.key === activeQueryKey">
				<p class="caption" contenteditable @input="event => onTabCaptionChanged(event, queryItem)">{{queryItem.caption}}</p>
				<span class="tab-header-item-action el-icon-close" @click="removeQuery(queryItem.key)"></span>
			</div>
		</div>
		<el-button size="small" @click="isFullScreen = !isFullScreen" icon="el-icon-full-screen"></el-button>
	</div>
	<div class="site-console-content">
		<sql-query-panel v-if="activeQuery" :app="app" :query="activeQuery" @onQuerySQLChange="onQuerySQLChange"></sql-query-panel>
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
import SQLQuery from "@/components/SQLQuery";


@Component({
	components: {
		SqlQueryPanel,
	},
})
export default class WebAppSqlPanel extends Vue {

	@Prop({required: true})
	public app: WebApp;

	public isFullScreen: boolean = false;

	public activeQueryKey: string | null = null;

	public queries: SQLQuery[] = [];

	public addQuery() {
		const query = new SQLQuery();
		this.queries.push(query);
		this.activeQueryKey = query.key;
		this.saveDataToCache();
	}

	public removeQuery(key: string) {
		const index = this.queries.findIndex((item) => item.key === key);
		this.queries.splice(index, 1);
		this.saveDataToCache();
	}

	public saveDataToCache() {
		const key = `${this.app.id}_sqlDataCache`;
		localStorage.setItem(key, JSON.stringify({
			isFullScreen: this.isFullScreen,
			queries: this.queries,
			activeQueryKey: this.activeQueryKey,
		}));
	}

	public loadDataFromCache() {
		const key = `${this.app.id}_sqlDataCache`;
		try {
			const storageItem = localStorage.getItem(key);
			const data = JSON.parse(storageItem || "") as {
				queries: Array<{key: string, sql: string, caption: string}>,
				isFullScreen: boolean,
				activeQueryKey: string;
			};
			this.isFullScreen = data.isFullScreen;
			this.queries = data.queries.map((item) => new SQLQuery(item));
			this.activeQueryKey = data.activeQueryKey;
		} catch {
			this.queries = [];
			this.addQuery();
		}
	}

	public get activeQuery() {
		return this.queries.find((item) => item.key === this.activeQueryKey);
	}


	public onTabCaptionChanged(event: InputEvent, query: SQLQuery) {
		const selection = window.getSelection();
		const cursorPosition = selection ? selection.anchorOffset : 0;
		let innerText = (event.srcElement as HTMLElement).innerText;
		if (!Boolean(innerText)) {
			innerText = "Q";
			(event.srcElement as HTMLElement).innerText = innerText;
		}
		query.caption = innerText;
		setTimeout(() => {
			const range = document.createRange();
			const textNode = (event.srcElement as HTMLElement).firstChild;
			if (!textNode) {
				return;
			}
			range.setStart(textNode, cursorPosition);
			range.collapse(true);
			const sel = window.getSelection();
			if (sel) {
				sel.removeAllRanges();
				sel.addRange(range);
			}
		}, 0);
	}

	public created() {
		this.loadDataFromCache();
	}

	protected onQuerySQLChange() {
		this.saveDataToCache();
	}

	@Watch("activeQueryKey")
	protected onChange_activeQueryKey() {
		this.saveDataToCache();
	}

	@Watch("isFullScreen")
	protected onChange_isFullScreen() {
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
		border-bottom: none;
		box-shadow: 0 0 5px 0px #aaa;
	}

	.web-app-sql-main-panel > *:not(.tab-panel) {
		margin: 5px;
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
    border-bottom: solid 2px #eee;
}


	.web-app-sql-main-panel > *:not(.tab-panel) {
		margin: 5px;
	}

.tab-header-item {
    user-select: none;
    height: 100%;
    margin-right: -1px;
    padding: 0 12px;
    box-sizing: border-box;
    line-height: 36px;
    display: inline-block;
    list-style: none;
    font-weight: 500;
    color: #303133;
    position: relative;
    border-bottom-color: #fff;
    border: 1px solid #e4e7ed;
	border-bottom: none;
	border-top: none;
    cursor: pointer;
}

.tab-header-item:hover {
    color: #aaf;
}
.tab-header-item:active {
    color: #66f;
}
.tab-panel {
	flex: 1;
	margin: 0 10px;
    white-space: nowrap;
    overflow-x: auto;
	overflow-y: hidden;
	&::-webkit-scrollbar {
		height: 4px;
	}
}
.tab-panel > * {
	display: inline-block;
}

.tab-header-item > .caption {
	
    display: inline-block;
    margin-block-start: 0;
    margin-block-end: 0;
    margin-inline-start: 0;
	margin-inline-end: 0;
	outline: none;
}


.tab-header-item > .tab-header-item-action {
	margin-left: 4px;
	color: #ddd;
	&:hover {
		background-color: #aaa;
		border-radius: 20px;
		color: #fff;
	}
}
.tab-header-item:hover, .tab-header-item[active] {
    background-color: #eee;
	color: #44d;
	border-color: #9ba0a9;
    z-index: 1;
	.tab-header-item-action {
		color: #44d;
	}
}
.add-tab-btn {
	
    margin-left: 10px;
    cursor: pointer;
}
</style>
