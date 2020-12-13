<template><div class="header-panel">
	<div>
		<el-input v-model="filterText" class="filter-text-input" clearable placeholder="пошук..." style="width:30vw;"></el-input>
		<el-button @click="filterOnlyActive = !filterOnlyActive" :type="filterOnlyActive ? 'success' : ''"
			style="margin-left: 10px;">ACTIVE</el-button>
		<!-- <el-input v-model="apiAddress" class="filter-text-input" style="width:30vw;"></el-input> -->
	</div>
	<div>
		<el-button type="info" plain @click="HelpVisible = true"><i class="el-icon-help"></i></el-button>
		<el-button type="primary" :plain="!dataLoading" @click="refresh"><i :class="dataLoading ? 'el-icon-loading' : 'el-icon-refresh'"></i></el-button>
		<el-select v-model="autoRefreshTime" style="width: 100px; margin-left: 10px;" value-key="value">
			<el-option v-for="item in AutoRefreshTimeList" :key="item.value" :value="item.value" :label="item.label"/>
		</el-select>
	</div>
</div></template>

<script lang="ts">
import Vue from "vue";
import Vuex from "vuex";

export default Vue.extend({
	name: "header-panel",
	methods: {
		refresh() {
			this.$emit("refresh");
		},
	},
	computed: {
		filterText: {
			get(): string { return this.$store.state.filterText; },
			set(value: string) { this.$store.commit("setFilterText", value); },
		},
		apiAddress: {
			get(): string { return this.$store.state.apiAddress; },
			set(value: string) { this.$store.commit("setApiAddress", value); },
		},
		filterOnlyActive: {
			get(): boolean { return this.$store.state.filterOnlyActive; },
			set(value: boolean) { this.$store.commit("setFilterOnlyActive", value); },
		},
		autoRefreshTime: {
			get(): boolean { return this.$store.state.autoRefreshTime; },
			set(value: boolean) { this.$store.commit("setAutoRefreshTime", value); },
		},
		HelpVisible: {
			get(): boolean { return this.$store.state.HelpVisible; },
			set(value: boolean) { this.$store.commit("setHelpVisible", value); },
		},
		dataLoading: {
			get(): boolean { return this.$store.state.dataLoading; },
		},
		AutoRefreshTimeList: {
			get(): Array<({value: number, label: string})> { return this.$store.state.AutoRefreshTimeList; },
		},
	},
});
</script>

<style lang="less">
.header-panel {
	display: flex;
	justify-content: space-between;
	position: fixed;
	width: 100%;
	top: 0;
	z-index: 1000;
	height: 40px;
	border-bottom: solid 1px #eee;
	box-shadow: 0 0 5px 0px #aaa;
	
	.filter-text-input .el-input__inner {
		border-radius: 0;
		border-bottom: none;
		border-top: none;
		border-left: none;
	}
}

</style>
