<template><div id="app">

	<el-dialog  :visible.sync="HelpVisible" top="10vh" width="950px" custom-class="help-dialog">
		<template v-slot:title>
			<div class="help-header-cnt">
				<div class="help-application-info">
					<span class="logo-icon"></span>
					<span class="logo-version">v{{VERSION}}</span>
					<a href="https://github.com/VitaliySimkin/bnz-webapp-manager" target="_blank" class="logo-github"></a>
				</div>
			</div>
		</template>
		<div class="help-content-cnt">
			<span>Панель призначена для відображення сайтів Creatio розгорнутих на сервері, доступом до БД, redis, отримання інформації про стан пулу застосунків та керування ним</span>
			<span><b>Пошук</b> - пошук за входженням в назву сайту</span>
			<span><b>ACTIVE</b> - відображення тільки тих додатків для яких є запущені процеси</span>
			<span><b>Кнопка "Оновити" та час автоматичного оновлення</b> - дозволяють оновити дані про стан пулу застосунків. Буде оновлена інформація про стан пулу застосунків та запущені ним процеси</span>
			<span>Дані про пули застосунків та процеси надається за запитом на сервер. Перелік розгорнутих сайтів кешується та оновлюється кожні 5 хвилин на сервері</span>
			<img src="./assets/screen/common.png" />
			<span>Відображається стан пулу застосунків та використана ним ОЗУ. Якщо в пула застосунків є запущені процеси то він буде більш виділений, а при наведені на нього мишкою буде відображений список повязаних процесів </span>
			<span>Дані по CPU поки що не відображають коректну інформацію</span>
			<img src="./assets/screen/apppool.png" />
			<span>Консоль SQL. Пітримує <b>Oracle</b> та <b>MSSQL</b></span>
			<img src="./assets/screen/sql.png" />
			<span><b>Get all values</b> - Отримати всі значення з БД Redis</span>
			<span><b>Get</b> - Отримати значення по ключу</span>
			<span><b>Set</b> - Встановити значення в БД</span>
			<span>При кліку по запису в таблиці ключ та значення автоматично проставляються в поля вводу</span>
			<img src="./assets/screen/redis.png" />
			<span><b>physicalPath</b> - шлях до застосунку на диску </span>
			<span><b>applicationPoolName</b> - назва пулу застосунків </span>
			<span><b>dbConnectionString</b> - рядок підключення до БД </span>
			<span><b>redisConnectionString</b> - рядок підключення до Redis </span>
			<img src="./assets/screen/details.png" />
			<img src="./assets/screen/actions.png" />
		</div>
	</el-dialog>

	<header-panel @refresh = "reloadApplicationPoolData">

	</header-panel>
	<div class="sites-list-cnt" :is-loading="dataLoading">
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

	@State("VERSION")
	private VERSION: string;

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
		return (!this.filterText || app.path.toLowerCase().includes(this.filterText.toLowerCase()))
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

.sites-list-cnt[is-loading] {
	animation: loading_background_color_an 1.5s cubic-bezier(1, 1, 0, 0.01) 1s infinite;
}

@keyframes loading_background_color_an {
	0%  {background-color: #ecf5ff; }
	50%  {background-color: #b3d8ff; }
	100%  {background-color: #ecf5ff; }
}


#app {}

.help-cnt {
	height: 50vh;
	overflow:auto;
	img {
		border: solid 1px #aaa;
	}
}

.help-application-info {
	user-select: none;
	margin: 4px 10px;
}

.logo-icon {
    background-image: url(./assets/logo.png);
    background-size: contain;
    display: inline-block;
    height: 25px;
    width: 25px;
}

.logo-github {
    background-image: url(./assets/github.png);
    height: 23px;
    display: inline-block;
    width: 65px;
    background-size: 117%;
    background-position-y: -2px;
    background-position-x: -7px;
    margin-left: 20px;
    vertical-align: top;
}

.logo-version {
	font-family:'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
	font-weight: bold;
    font-size: 15px;
    display: inline-block;
    color: #555;
    height: 100%;
    padding-left: 5px;
    vertical-align: super;
}

.help-dialog {
	.el-dialog__body {
		padding: 0;
	}
}

.help-content-cnt {
	height: 70vh;
	overflow:auto;
	padding: 5px 20px;
	font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    font-size: 17px;
	font-weight: 400;
	word-break: break-word;
	span {
		display: block;
	}
	img {
		border: solid 1px #aaa;
		width: 98%;
		margin: 5px 0;
	}
}

</style>
