import { Controller } from '@nestjs/common';
import { DbClientService } from 'dal/db-client.service';

@Controller('questions')
export class QuestionsController {
  constructor(private readonly dbClientService: DbClientService) {}
}
