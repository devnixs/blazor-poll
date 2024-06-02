import { Module } from '@nestjs/common';
import { ConfigModule } from '@nestjs/config';
import { DbClientService } from 'dal/db-client.service';
import { TemplatesController } from './templates/templates.controller';

@Module({
  imports: [ConfigModule.forRoot()],
  controllers: [TemplatesController],
  providers: [DbClientService],
})
export class AppModule {}
