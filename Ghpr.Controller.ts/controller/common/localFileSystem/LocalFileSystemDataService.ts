﻿///<reference path="./../../interfaces/IDataService.ts"/>
///<reference path="./../../dto/RunDto.ts"/>
///<reference path="./../../dto/ReportSettingsDto.ts"/>
///<reference path="./../../dto/TestRunDto.ts"/>
///<reference path="./../../dto/ItemInfoDto.ts"/>
///<reference path="./../../dto/TestScreenshotDto.ts"/>
///<reference path="./entities/Run.ts"/>
///<reference path="./entities/ItemInfo.ts"/>
///<reference path="./mappers/RunDtoMapper.ts"/>
///<reference path="./mappers/ItemInfoDtoMapper.ts"/>
///<reference path="./mappers/TestScreenshotDtoMapper.ts"/>
///<reference path="./../../enums/PageType.ts"/>
///<reference path="./../ProgressBar.ts"/>
///<reference path="./../JsonParser.ts"/>

class LocalFileSystemDataService implements IDataService {

    private currentPage: PageType;
    private progressBar: ProgressBar;
    reportSettings: ReportSettingsDto;
    reviveRun = JsonParser.reviveRun;

    constructor() {
        this.progressBar = new ProgressBar(1);
    }

    fromPage(pageType: PageType): IDataService {
        this.currentPage = pageType;
        return this;
    }

    getRun(runGuid: string, callback: (runDto: RunDto) => void): void {
        const path = LocalFileSystemPathsHelper.getRunPath(this.currentPage, runGuid);
        this.loadJsonsByPaths([path], 0, new Array(), false, true, (response: string) => {
            const run: Run = JSON.parse(response, this.reviveRun);
            const runDto = RunDtoMapper.map(run);
            if (runDto.name === "") {
                runDto.name = `${DateFormatter.format(runDto.runInfo.start)} - ${DateFormatter.format(runDto.runInfo.finish)}`;
            }
            callback(runDto);
        });
    }

    getRunInfos(callback: (runInfoDtos: ItemInfoDto[]) => void): void {
        const path = LocalFileSystemPathsHelper.getRunsPath(this.currentPage);
        this.loadJsonsByPaths([path], 0, new Array(), false, true, (response: string) => {
            const runInfos: Array<ItemInfo> = JSON.parse(response, this.reviveRun);
            const len = runInfos.length;
            let runInfoDtos: Array<ItemInfoDto> = new Array(len);
            for (let i = 0; i < len; i++) {
                runInfoDtos[i] = ItemInfoDtoMapper.map(runInfos[i]);
            }
            runInfoDtos.sort(Sorter.itemInfoByFinishDateDesc);
            callback(runInfoDtos);
        });
    }

    getTestInfos(testGuid: string, callback: (testInfoDtos: ItemInfoDto[]) => void): void {
        const path = LocalFileSystemPathsHelper.getTestsPath(testGuid, this.currentPage);
        this.loadJsonsByPaths([path], 0, new Array(), false, true, (response: string) => {
            const testInfos: Array<ItemInfo> = JSON.parse(response, this.reviveRun);
            const len = testInfos.length;
            let testInfoDtos: Array<ItemInfoDto> = new Array(len);
            for (let i = 0; i < len; i++) {
                testInfoDtos[i] = ItemInfoDtoMapper.map(testInfos[i]);
            }
            testInfoDtos.sort(Sorter.itemInfoByFinishDateDesc);
            callback(testInfoDtos);
        });
    }

    getLatestRuns(callback: (runDtos: Array<RunDto>, total: number) => void): void {
        const path = LocalFileSystemPathsHelper.getRunsPath(this.currentPage);
        this.loadJsonsByPaths([path], 0, new Array(), false, true, (response: string) => {
            let runInfos: Array<ItemInfo> = JSON.parse(response, this.reviveRun);
            runInfos = runInfos.sort(Sorter.itemInfoByFinishDateDesc);
            let totalCount = runInfos.length;
            const runsToLoad = this.reportSettings.runsToDisplay >= 1 ? Math.min(this.reportSettings.runsToDisplay, runInfos.length) : runInfos.length;
            let runInfosDto: Array<ItemInfoDto> = new Array(runsToLoad);
            for (let i = 0; i < runsToLoad; i++) {
                runInfosDto[i] = ItemInfoDtoMapper.map(runInfos[i]);
            }
            const paths: Array<string> = new Array();
            for (let i = 0; i < runsToLoad; i++) {
                paths[i] = `runs/run_${runInfosDto[i].guid}.json`;
            }
            const runs: Array<RunDto> = new Array();
            this.loadJsonsByPaths(paths, 0, new Array(), false, false, (responses: Array<string>) => {
                for (let i = 0; i < responses.length; i++) {
                    const loadedRun: Run = JSON.parse(responses[i], this.reviveRun);
                    if (loadedRun.name === "") {
                        loadedRun.name = `${DateFormatter.format(loadedRun.runInfo.start)} - ${DateFormatter.format(loadedRun.runInfo.finish)}`;
                    }
                    runs[i] = RunDtoMapper.map(loadedRun);
                }
                callback(runs, totalCount);
            });
        });
    }

    getLatestTests(testGuid: string, callback: (testRunDtos: Array<TestRunDto>, total: number) => void): void {
        const path = LocalFileSystemPathsHelper.getTestsPath(testGuid, this.currentPage);
        this.loadJsonsByPaths([path], 0, new Array(), false, true, (response: string) => {
            let testInfos: Array<ItemInfo> = JSON.parse(response, this.reviveRun);
            testInfos = testInfos.sort(Sorter.itemInfoByFinishDateDesc);
            let totalCount = testInfos.length;
            const testsToLoad = this.reportSettings.testsToDisplay >= 1 ? Math.min(this.reportSettings.testsToDisplay, testInfos.length) : testInfos.length;
            let testInfosDto: Array<ItemInfoDto> = new Array(testsToLoad);
            for (let i = 0; i < testsToLoad; i++) {
                testInfosDto[i] = ItemInfoDtoMapper.map(testInfos[i]);
            }
            const paths: Array<string> = new Array();
            for (let i = 0; i < testsToLoad; i++) {
                paths[i] = LocalFileSystemPathsHelper.getTestItemPath(testInfosDto[i].itemName, testInfosDto[i].guid, this.currentPage);
            }
            const testRuns: Array<TestRunDto> = new Array();
            this.loadJsonsByPaths(paths, 0, new Array(), false, false, (responses: Array<string>) => {
                for (let i = 0; i < responses.length; i++) {
                    const loadedTestRun: TestRun = JSON.parse(responses[i], this.reviveRun);
                    testRuns[i] = TestRunDtoMapper.map(loadedTestRun);
                }
                callback(testRuns, totalCount);
            });
        });
    }

    getLatestTest(testGuid: string, itemName: string, callback: Function): void {
        const path = LocalFileSystemPathsHelper.getTestItemPath(itemName, testGuid, this.currentPage);
        this.loadJsonsByPaths([path], 0, new Array(), false, true, (response: string) => {
            const testRun: TestRun = JSON.parse(response, this.reviveRun);
            const testRunDto = TestRunDtoMapper.map(testRun);
            callback(testRunDto);
        });
    }
    
    getTestOutput(t: TestRunDto, callback: Function): void {
        const path = LocalFileSystemPathsHelper.getTestItemPath(t.output.itemName, t.testInfo.guid, this.currentPage);
        this.loadJsonsByPaths([path], 0, new Array(), false, true, (response: string) => {
            const testRun: TestOutput = JSON.parse(response, this.reviveRun);
            const testRunDto = TestOutputDtoMapper.map(testRun);
            callback(testRunDto);
        });
    }

    getTestScreenshots(testRunDto: TestRunDto, callback: (screenshotDto: Array<TestScreenshotDto>) => void): void {
        const paths: Array<string> = new Array();
        const screensInfo = testRunDto.screenshots;
        for (let j = 0; j < screensInfo.length; j++) {
            paths[j] = `./../tests/${testRunDto.testInfo.guid}/img/${screensInfo[j].itemName}`;
        }
        const testScreenshots: Array<TestScreenshot> = new Array();
        this.loadJsonsByPaths(paths, 0, new Array(), false, false, (responses: Array<string>) => {
            for (let i = 0; i < responses.length; i++) {
                const loadedScreenshot: TestScreenshot = JSON.parse(responses[i], this.reviveRun);
                testScreenshots[i] = TestScreenshotDtoMapper.map(loadedScreenshot);
            }
            callback(testScreenshots);
        });
    }

    getRunTests(runDto: RunDto, callback: (testRunDto: TestRunDto, c: number, i: number) => void): void {
        const paths: Array<string> = new Array();
        var test: TestRun;
        var testDto: TestRunDto;
        const testsInfo = runDto.testsInfo;
        for (let j = 0; j < testsInfo.length; j++) {
            paths[j] = `./../tests/${testsInfo[j].guid}/${testsInfo[j].itemName}`;
        }
        this.loadJsonsByPaths(paths, 0, new Array(), true, true, (response: string, c: number, i: number) => {
            test = JSON.parse(response, this.reviveRun);
            testDto = TestRunDtoMapper.map(test);
            callback(testDto, c, i);
        });
    }

    loadJsonsByPaths(paths: Array<string>, ind: number, responses: Array<string>, showProgressBar: boolean, callbackForEach: boolean, callback: Function): void {
        const count = paths.length;
        if (showProgressBar) {
            this.progressBar.reset(count);
            if (ind === 0) {
                this.progressBar.show();
            }
            if (ind >= count) {
                this.progressBar.hide();
                return;
            }
        }
        if (!callbackForEach && ind >= count) {
            callback(responses, count, ind);
        }
        if (ind >= count) {
            return;
        }
        const req = new XMLHttpRequest();
        req.overrideMimeType("application/json");
        req.open("get", paths[ind], true);
        req.onreadystatechange = () => {
            if (req.readyState === 4)
                if (req.status !== 200 && req.status !== 0) {
                    console
                        .log(`Error while loading .json data: '${paths[ind]}'! Request status: ${req.status} : ${req.statusText}`);
                } else {
                    responses[ind] = req.responseText;
                    if (callbackForEach) {
                        callback(req.responseText, count, ind);
                    }
                    if (showProgressBar) {
                        this.progressBar.onLoaded(ind);
                    }
                    ind++;
                    this.loadJsonsByPaths(paths, ind, responses, showProgressBar, callbackForEach, callback);
                }
        }
        req.timeout = 2000;
        req.ontimeout = () => {
            console.log(`Timeout while loading .json data: '${paths[ind]}'! Request status: ${req.status} : ${req.statusText}`);
        };
        req.send(null);
    }
}