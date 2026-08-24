import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import customParseFormat from 'dayjs/plugin/customParseFormat';
import dayjsCustomPlugin from '@/setup/dayjs-custom-plugin.js';
import objectSupport from 'dayjs/plugin/objectSupport';
import advancedFormat from 'dayjs/plugin/advancedFormat';

dayjs.extend(utc);
dayjs.extend(customParseFormat);
dayjs.extend(dayjsCustomPlugin);
dayjs.extend(objectSupport);
dayjs.extend(advancedFormat);