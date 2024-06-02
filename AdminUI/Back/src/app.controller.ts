import { Body, Controller, Get, Post, Query, Req } from '@nestjs/common';
import { AppService } from './app.service';
import { Request } from 'express';
import { BodyDto } from '../dtos/BodyDto';

@Controller('foo')
export class AppController {
  constructor(private readonly appService: AppService) {}

  @Get('bar')
  getHello(@Req() request: Request, @Query('param') param: string): any {
    return {
      data: this.appService.getHello(),
      param: param,
    };
  }

  @Post('bar')
  postTest(@Body() body: BodyDto): any {
    return {
      data: body,
    };
  }
}
