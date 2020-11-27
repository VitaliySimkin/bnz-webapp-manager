import Vue from "vue";
import Vuex from "vuex";
import { WebApp, ApplicationPoolData } from "../../../api-client/index";

Vue.use(Vuex);

const AutoRefreshTimeList = [
	{ value: -1, label: "OFF" },
	{ value: 2, label: "2 сек" },
	{ value: 5, label: "5 сек" },
	{ value: 10, label: "10 сек" },
	{ value: 30, label: "30 сек" },
	{ value: 60, label: "1 хв" },
];

class State {
	public filterText: string = "";
	public filterOnlyActive: boolean = false;
	public applications: WebApp[] = [];
	public appPools: ApplicationPoolData[] = [];
	public autoRefreshTime: number = -1;
	public AutoRefreshTimeList = AutoRefreshTimeList;
	public dataLoading: boolean = false;
	public HelpVisible: boolean = false;
	public VERSION: string = "1.1.0";
	public CONFIG: object = {};
}

const STORE = new Vuex.Store({
	state: new State(),
	mutations: {
		setFilterText(state, value) {
			state.filterText = value;
		},
		setFilterOnlyActive(state, value) {
			state.filterOnlyActive = value;
		},
		setApplications(state, apps: WebApp[]) {
			state.applications = apps;
		},
		setHelpVisible(state, value) {
			state.HelpVisible = value;
		},
		setPools(state, pools: ApplicationPoolData[]) {
			state.appPools = pools;
		},
		setAutoRefreshTime(state, value) {
			state.autoRefreshTime = value || -1;
		},
		setDataLoading(state, value) {
			state.dataLoading = Boolean(value);
		},
		setConfig(state, config) {
			state.CONFIG = config;
		},
	},
	actions: {
	},
	modules: {
	},
});


(async function loadConfig() {
	// const data = await fetch("../config.json");
	// const config = await data.json();
	// STORE.commit("setConfig", config);
})();

export default STORE;
