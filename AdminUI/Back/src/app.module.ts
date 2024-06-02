import { Module } from '@nestjs/common';
import { AppController } from './app.controller';
import { AppService } from './app.service';
import { StartupService } from './startup.service';

@Module({
  imports: [],
  controllers: [AppController],
  providers: [AppService, StartupService],
})
export class AppModule {}
