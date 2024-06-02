import { AppService } from './app.service';
import { Request } from 'express';
import { BodyDto } from '../dtos/BodyDto';
export declare class AppController {
    private readonly appService;
    constructor(appService: AppService);
    getHello(request: Request, param: string): any;
    postTest(body: BodyDto): any;
}
