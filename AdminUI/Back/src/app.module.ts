import { Module } from '@nestjs/common';
import { ConfigModule } from '@nestjs/config';
import { DbClientService } from 'dal/db-client.service';
import { QuestionsController } from './questions/questions.controller';
import { TemplatesController } from './templates/templates.controller';

@Module({
  imports: [ConfigModule.forRoot()],
  controllers: [QuestionsController, TemplatesController],
  providers: [DbClientService],
})
export class AppModule {}
