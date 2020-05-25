<template><div id="app">

	<el-dialog title="Довідка" :visible.sync="HelpVisible" width="80%">
		<div class="help-cnt" style="height: 50vh;overflow:auto;">
			<img src="./assets/screen/common.png" />
			<img src="./assets/screen/apppool.png" />
			<img src="./assets/screen/sql.png" />
			<img src="./assets/screen/redis.png" />
			<img src="./assets/screen/details.png" />
			<img src="./assets/screen/actions.png" />
		</div>
	</el-dialog>

	<header-panel @refresh = "reloadApplicationPoolData">

	</header-panel>
	<div class="sites-list-cnt">
		<web-app-cnt v-for="app in applications" :key="app.id" :app="app" v-show="isVisible(app)" />
	</div>
</div></template>

<script lang="ts">
import Vue from "vue";
import Vuex from "vuex";
import ElementUI from "element-ui";
import HeaderPanel from "./components/header-panel.vue";
import WebAppCnt from "./components/web-app-cnt.vue";
import { Component, Prop, Watch } from "vue-property-decorator";
import { State, Mutation } from "vuex-class";
// tslint:disable-next-line
const locale = require("element-ui/lib/locale/lang/ua.js");
import "element-ui/lib/theme-chalk/index.css";

import { WebAppApi, WebApp, ApplicationPoolData, ApplicationPoolApi } from "../../api-client/index";
Vue.use(ElementUI, { locale });

@Component({
	components: {
		HeaderPanel,
		WebAppCnt,
	},
	computed: {
		HelpVisible: {
			get(): boolean { return this.$store.state.HelpVisible; },
			set(value: boolean) { this.$store.commit("setHelpVisible", value); },
		},
	},
})
export default class App extends Vue {

	@State("appPools")
	private appPools: ApplicationPoolData[];

	@State("applications")
	private applications: WebApp[];

	@State("filterText")
	private filterText: string;

	@State("filterOnlyActive")
	private filterOnlyActive: boolean;

	@State("dataLoading")
	private dataLoading: boolean;

	@State("autoRefreshTime")
	private autoRefreshTime: number;

	private autoRefreshIntervalId: number | null = null;

	public created() {
		this.initData();
	}

	protected getApplicationIsActive(app: WebApp) {
		const pool = this.appPools.find(({name}) => name === app.applicationPoolName);
		return pool && pool.workerProcesses.length > 0;
	}

	protected async loadApplications() {
		this.$store.commit("setApplications", await new WebAppApi().get());
	}

	protected showHelp() {
		this.$store.commit("setHelpVisible", true);
	}

	protected async loadPoolsData() {
		this.$store.commit("setDataLoading", true);
		this.$store.commit("setPools", await new ApplicationPoolApi().getApplicationPools());
		this.$store.commit("setDataLoading", false);
	}

	protected async initData() {
		await Promise.all([this.loadApplications(), this.loadPoolsData()]);
		this.startAutoRefresh();
	}

	@Watch("autoRefreshTime")
	protected startAutoRefresh() {
		this.stopAutoRefresh();
		if (this.autoRefreshIntervalId || this.autoRefreshTime <= 0) {
			return;
		}
		this.autoRefreshIntervalId = setInterval(this.reloadApplicationPoolData.bind(this), this.autoRefreshTime * 1000);
	}

	protected stopAutoRefresh() {
		if (this.autoRefreshIntervalId) {
			clearInterval(this.autoRefreshIntervalId);
			this.autoRefreshIntervalId = null;
		}
	}

	protected reloadApplicationPoolData() {
		this.loadPoolsData();
	}

	protected isVisible(app: WebApp) {
		return (!this.filterText || app.path.includes(this.filterText))
			&& (!this.filterOnlyActive || this.getApplicationIsActive(app));
	}
}
</script>

<style lang="less">

body, html {
	margin: 0;
	padding: 0;
}

#app {
	padding-top: 40px;
}

.sites-list-cnt {
	padding: 10px;
	background-color: #fff;
	box-sizing: border-box;
	height: calc(100vh - 40px);
	overflow: auto;
}

#app {}

.help-cnt {
	height: 50vh;
	overflow:auto;
	img {
		border: solid 1px #aaa;
	}
}

</style>
