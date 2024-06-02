import {
  Body,
  Controller,
  Get,
  HttpException,
  HttpStatus,
  Param,
  Post,
} from '@nestjs/common';
import { ApiOkResponse } from '@nestjs/swagger';
import { GameTemplates } from '@prisma/client';
import { DbClientService } from 'dal/db-client.service';
import { ApiOkResponsePaginated, PagingDto } from 'dtos/common.dto';
import {
  GameTemplateDetailDto,
  GameTemplateDto,
  ListTemplatesRequestDto,
} from 'dtos/templates.dto';

@Controller('templates')
export class TemplatesController {
  constructor(private readonly dbClientService: DbClientService) {}

  @Get('list')
  @ApiOkResponsePaginated(GameTemplateDto)
  async listTemplates(
    @Body() body: ListTemplatesRequestDto,
  ): Promise<PagingDto<GameTemplates>> {
    const item = await this.dbClientService.listTemplates(body.page, 10);
    return new PagingDto<GameTemplates>(item.data, body.page, 10, item.count);
  }

  @Get('get/:id')
  @ApiOkResponse({
    type: GameTemplateDetailDto,
  })
  async getTemplate(@Param('id') id: number): Promise<GameTemplateDetailDto> {
    const item = await this.dbClientService.getTemplate(id);

    return new GameTemplateDetailDto(item.template, item.questions);
  }

  @Post('update')
  @ApiOkResponse({
    type: GameTemplateDetailDto,
  })
  async updateTemplate(
    @Body() template: GameTemplateDetailDto,
  ): Promise<GameTemplateDetailDto> {
    this.validateTemplate(template);
    await this.dbClientService.updateTemplate(template);
    const item = await this.dbClientService.getTemplate(template.template.id);

    return new GameTemplateDetailDto(item.template, item.questions);
  }

  @Post('create')
  @ApiOkResponse({
    type: GameTemplateDetailDto,
  })
  async createTemplate(
    @Body() template: GameTemplateDetailDto,
  ): Promise<GameTemplateDetailDto> {
    this.validateTemplate(template);
    const created = await this.dbClientService.createTemplate(template);
    const templateAndQuestions = await this.dbClientService.getTemplate(
      created.id,
    );

    return new GameTemplateDetailDto(
      templateAndQuestions.template,
      templateAndQuestions.questions,
    );
  }

  validateTemplate(template: GameTemplateDetailDto) {
    if (!template.template.name) {
      throw new HttpException('Name is required', HttpStatus.BAD_REQUEST);
    }
    if (template.questions.length === 0) {
      throw new HttpException(
        'At least one question is required',
        HttpStatus.BAD_REQUEST,
      );
    }
    template.questions.forEach((q) => {
      if (!q.content) {
        throw new HttpException(
          'Question content is required',
          HttpStatus.BAD_REQUEST,
        );
      }
      if (q.choices.length === 0) {
        throw new HttpException(
          'At least one choice is required',
          HttpStatus.BAD_REQUEST,
        );
      }
      q.choices.forEach((c) => {
        if (!c.content) {
          throw new HttpException(
            'Choice content is required',
            HttpStatus.BAD_REQUEST,
          );
        }
      });
      if (q.choices.filter((c) => c.isValid).length !== 1) {
        throw new HttpException(
          'Only one choice can be valid',
          HttpStatus.BAD_REQUEST,
        );
      }
    });
  }
}
