"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var __param = (this && this.__param) || function (paramIndex, decorator) {
    return function (target, key) { decorator(target, key, paramIndex); }
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.AppController = void 0;
const common_1 = require("@nestjs/common");
const app_service_1 = require("./app.service");
const BodyDto_1 = require("../dtos/BodyDto");
let AppController = class AppController {
    constructor(appService) {
        this.appService = appService;
    }
    getHello(request, param) {
        return {
            data: this.appService.getHello(),
            param: param,
        };
    }
    postTest(body) {
        return {
            data: body,
        };
    }
};
exports.AppController = AppController;
__decorate([
    (0, common_1.Get)('bar'),
    __param(0, (0, common_1.Req)()),
    __param(1, (0, common_1.Query)('param')),
    __metadata("design:type", Function),
    __metadata("design:paramtypes", [Object, String]),
    __metadata("design:returntype", Object)
], AppController.prototype, "getHello", null);
__decorate([
    (0, common_1.Post)('bar'),
    __param(0, (0, common_1.Body)()),
    __metadata("design:type", Function),
    __metadata("design:paramtypes", [BodyDto_1.BodyDto]),
    __metadata("design:returntype", Object)
], AppController.prototype, "postTest", null);
exports.AppController = AppController = __decorate([
    (0, common_1.Controller)('foo'),
    __metadata("design:paramtypes", [app_service_1.AppService])
], AppController);
//# sourceMappingURL=app.controller.js.map