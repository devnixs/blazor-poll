import { Min } from 'class-validator';
import { PagingDto } from './common.dto';
import { ApiProperty } from '@nestjs/swagger';

export class ListTemplatesRequestDto {
  @Min(1)
  @ApiProperty()
  page: number;
}

export class GameTemplateDto {
  @ApiProperty()
  id: number;

  @ApiProperty()
  creationDate: Date;

  @ApiProperty()
  name: string;
}

export class ListTemplatesResponseDto extends PagingDto<GameTemplateDto> {}

export class ChoiceDto {
  @ApiProperty()
  id: number;

  @ApiProperty()
  content: string;

  @ApiProperty()
  index: number;

  @ApiProperty()
  isValid: boolean;
}

export class QuestionDto {
  @ApiProperty()
  id: number;

  @ApiProperty()
  content: string;

  @ApiProperty()
  gameTemplateId: number;

  @ApiProperty()
  index: number;

  @ApiProperty()
  askingQuestionImageUrl: string;

  @ApiProperty()
  presentingAnswerImageUrl: string;

  @ApiProperty({ type: [ChoiceDto] })
  choices: ChoiceDto[];
}

export class GameTemplateDetailDto {
  constructor(template: GameTemplateDto, questions: QuestionDto[]) {
    this.template = template;
    this.questions = questions;
  }

  @ApiProperty({ type: [GameTemplateDto] })
  template: GameTemplateDto;

  @ApiProperty({ type: [QuestionDto] })
  questions: QuestionDto[];
}
