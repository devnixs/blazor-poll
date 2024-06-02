import { Injectable, Logger } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import { GameTemplates, PrismaClient, QuestionChoice } from '@prisma/client';
import { GameTemplateDetailDto, QuestionDto } from 'dtos/templates.dto';

@Injectable()
export class DbClientService {
  private readonly logger = new Logger(DbClientService.name);
  private _prisma: PrismaClient;
  constructor(configService: ConfigService) {
    const connectionString = configService.get('DATABASE_URL');
    this._prisma = new PrismaClient({
      datasources: {
        db: {
          url: connectionString,
        },
      },
      log: ['query'],
    });
  }

  async onModuleInit() {
    await this._prisma.$connect();
    this.logger.log('Connected to database');
  }

  async listTemplates(
    page: number,
    pageSize: number,
  ): Promise<{ data: GameTemplates[]; count: number }> {
    const templates = await this._prisma.gameTemplates.findMany({
      skip: (page - 1) * pageSize,
      take: pageSize,
    });
    const count = await this._prisma.gameTemplates.count();

    return { data: templates, count: count };
  }

  async getTemplate(id: number): Promise<{
    template: GameTemplates;
    questions: QuestionDto[];
  }> {
    const templates = await this._prisma.gameTemplates.findUnique({
      where: { id: id },
    });

    const questions = await this._prisma.questions.findMany({
      where: { gameTemplateId: id },
    });

    const choices = await this._prisma.questionChoice.findMany({
      where: { questionId: { in: questions.map((q) => q.id) } },
    });

    const questionsDtos = questions.map((q) => {
      const dto = new QuestionDto();
      dto.id = q.id;
      dto.content = q.content;
      dto.gameTemplateId = q.gameTemplateId;
      dto.index = q.index;
      dto.askingQuestionImageUrl = q.askingQuestionImageUrl;
      dto.presentingAnswerImageUrl = q.presentingAnswerImageUrl;
      dto.choices = choices.filter((c) => c.questionId === q.id);
      return dto;
    });

    return { template: templates, questions: questionsDtos };
  }

  async updateTemplate(obj: GameTemplateDetailDto): Promise<void> {
    await this._prisma.gameTemplates.update({
      data: obj.template,
      where: { id: obj.template.id },
    });

    const existingQuestions = await this._prisma.questions.findMany({
      where: { gameTemplateId: obj.template.id },
    });
    const existingChoices = await this._prisma.questionChoice.findMany({
      where: { questionId: { in: existingQuestions.map((q) => q.id) } },
    });

    await this._prisma.questions.deleteMany({
      where: { gameTemplateId: { in: existingQuestions.map((i) => i.id) } },
    });

    await this._prisma.questionChoice.deleteMany({
      where: { questionId: { in: existingChoices.map((q) => q.id) } },
    });

    for (let i = 0; i < obj.questions.length; i++) {
      const question = {
        ...obj.questions[i],
        gameTemplateId: obj.template.id,
      };
      question[i].index = i;
      const newlyCreated = await this._prisma.questions.create({
        data: question,
      });

      const choices: QuestionChoice[] = [];
      for (let i = 0; i < question.choices.length; i++) {
        const choice = {
          ...question.choices[i],
          questionId: newlyCreated.id,
        };
        choice.index = i;
        choice.questionId = newlyCreated.id;
        choices.push(choice);
      }

      await this._prisma.questionChoice.createMany({
        data: choices,
      });
    }
  }

  async createTemplate(obj: GameTemplateDetailDto): Promise<GameTemplates> {
    const template = await this._prisma.gameTemplates.create({
      data: obj.template,
    });

    for (let i = 0; i < obj.questions.length; i++) {
      const question = obj.questions[i];
      question.index = i;
      question.gameTemplateId = template.id;
      const newlyCreated = await this._prisma.questions.create({
        data: question,
      });

      const choices: QuestionChoice[] = [];
      for (let i = 0; i < question.choices.length; i++) {
        const choice = {
          ...question.choices[i],
          questionId: newlyCreated.id,
        };
        choice.index = i;
        choice.questionId = newlyCreated.id;
        choices.push(choice);
      }

      await this._prisma.questionChoice.createMany({
        data: choices,
      });
    }
    return template;
  }
}
